using System;
using System.Collections.Generic;
using Streamiz.Kafka.Net.Processors.Public;
using Streamiz.Kafka.Net.State;
using Streamiz.Kafka.Net.State.Enumerator;

namespace de_duplication
{
    public class DeduplicationTransformer : ITransformer<string, MappedEventValue, string, MappedEventValue>
    {
        private ProcessorContext<string, MappedEventValue> _context;
        private IWindowStore<string, MappedEventValue> _eventIdStore;
        private readonly string _storeName;
        private readonly double _leftDurationMs;
        private readonly double _rightDurationMs;

        public DeduplicationTransformer()
        {

        }

        public DeduplicationTransformer(string storeName, double maintainDurationPerEventInMs)
        {
            if (maintainDurationPerEventInMs < 1)
            {
                throw new ArgumentException("Maintain duration per event must be >= 1");
            }
            _leftDurationMs = maintainDurationPerEventInMs / 2;
            _rightDurationMs = maintainDurationPerEventInMs - _leftDurationMs;
            this._storeName = storeName;
        }

        public void Init(ProcessorContext<string, MappedEventValue> context)
        {
            _context = context;
            _eventIdStore = (IWindowStore<string, MappedEventValue>)context.GetStateStore(_storeName);
        }

        public Record<string, MappedEventValue> Process(Record<string, MappedEventValue> record)
        {
            if (record.Key == null || record.Value == null)
            {
                return record;
            }
            else
            {
                if (IsDuplicate(record.Key, record))
                {
                    // Discard the record.
                    return null;
                }

                PutEventInStore(record.Value.FinalKey, record.Value, _context.Timestamp);
                return record;
            }
        }

        private bool IsDuplicate(string eventKey, Record<string, MappedEventValue> record)
        {
            long eventTime = _context.Timestamp;
            IWindowStoreEnumerator<MappedEventValue> timeIterator = _eventIdStore.Fetch(
                eventKey,
                eventTime - Convert.ToInt64(_leftDurationMs),
                eventTime + Convert.ToInt64(_rightDurationMs)
            );
            
            bool isDuplicate = timeIterator.MoveNext();

            timeIterator.Dispose();
            return isDuplicate;
        }

        private void PutEventInStore(string eventKey, MappedEventValue value, long timestamp)
        {
            _eventIdStore.Put(eventKey, value, timestamp);
        }

        public void Close()
        {
            
        }
    }
}