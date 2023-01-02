# Intro

The idea of this sample is to illustrate how you could join events with some referential data.

Let's present the use case : We work for a shoes e-shop online, all orders are published into a Kafka topic. Some referential data like the customer name, email, the product name, the price are not present in the order payload.

As a developer, you want to publish an enriched event with all these fields into a Kafka topic for the downstream applications.

![flow](./resources/streamiz-flow.png)

Customers, Products and Orders referential will be fed by three datagen connectors.

# How to start ? 

Run the start script
``` bash
./start.sh
```

Wait this log appears in the console 
```logs
"🚀 The deployment is complete. Please run the ./start-connectors.sh .. "
```

Open a new terminal and start the connectors to generate data in the source topics
```
./start-connectors.sh
```

Inspect the output in the first opened terminal, you should see a flow of records with the enriched payload (order, customer and product) like this :

``` logs
f0291a12-5592-4c07-bb96-3c5288c13208    {"OrderId":21598,"ProductId":"fb0bcd57-1c67-44c6-bfc0-c947c381a6b5","ProductName":"Max TrailRunner 209","ProductBrand":"Braun-Bruen","ProductPrice":12995,"CustomerId":"f0291a12-5592-4c07-bb96-3c5288c13208","OrderTime":"2021-01-24T20:10:00Z","CustomerFirstName":"Allene","CustomerLastName":"Peck","CustomerEmail":"kperfili7n@dell.com","CustomerPhone":"603-343-0835","CustomerAddress":"24141 Dryden Plaza 92878 California - United States"}
d490a965-c3c7-4db4-b334-6b2ee3f9c6e9    {"OrderId":21594,"ProductId":"42ef6bb1-17be-4798-896d-ffe972628f8c","ProductName":"Pro Spectra5 359","ProductBrand":"Macejkovic, Walter and Cummings","ProductPrice":15995,"CustomerId":"d490a965-c3c7-4db4-b334-6b2ee3f9c6e9","OrderTime":"2021-01-24T20:03:20Z","CustomerFirstName":"Fran","CustomerLastName":"Fleetham","CustomerEmail":"velton1c@boston.com","CustomerPhone":"253-614-0334","CustomerAddress":"21 Longview Street 67230 Kansas - United States"}
7e465e87-8d5a-4c0e-a596-b5ab2d74955b    {"OrderId":21602,"ProductId":"eb8b7807-77a1-47fc-9d54-ff3007638ff6","ProductName":"Impreza Max 680","ProductBrand":"Tillman, Effertz and Nikolaus","ProductPrice":6495,"CustomerId":"7e465e87-8d5a-4c0e-a596-b5ab2d74955b","OrderTime":"2021-01-24T20:16:40Z","CustomerFirstName":"Alena","CustomerLastName":"Sabine","CustomerEmail":"smatusson76@bandcamp.com","CustomerPhone":"506-340-3433","CustomerAddress":"23651 Westerfield Junction 7104 New Jersey - United States"}
eed82e84-4776-4d3e-83e2-4f90b9ea61e2    {"OrderId":21600,"ProductId":"eafbde70-0a6c-4b76-a769-a6e4ad3d6a34","ProductName":"Perf Impreza 795","ProductBrand":"Braun-Bruen","ProductPrice":10197,"CustomerId":"eed82e84-4776-4d3e-83e2-4f90b9ea61e2","OrderTime":"2021-01-24T20:13:20Z","CustomerFirstName":"Loutitia","CustomerLastName":"Rivalant","CustomerEmail":"smoberley55@spotify.com","CustomerPhone":"190-251-4179","CustomerAddress":"712 Hermina Pass 47937 Indiana - United States"}
f88fdd10-ef3d-460a-a7fd-5dcc87571be6    {"OrderId":21599,"ProductId":"88c1cd12-54f8-47da-b660-204735dd0c05","ProductName":"Spectra5 Pro 870","ProductBrand":"Oberbrunner, Dietrich and Hickle","ProductPrice":6297,"CustomerId":"f88fdd10-ef3d-460a-a7fd-5dcc87571be6","OrderTime":"2021-01-24T20:11:40Z","CustomerFirstName":"Marlee","CustomerLastName":"Grinval","CustomerEmail":"klongman30@yale.edu","CustomerPhone":"667-492-9839","CustomerAddress":"04918 Service Circle 11044 New York - United States"}
1e42e798-bdea-4de2-8f92-9766c358d471    {"OrderId":21603,"ProductId":"dd1d0b6a-adc2-417e-b100-972b357b160a","ProductName":"Spectra5 Perf 837","ProductBrand":"Beer, DAmore and Wintheiser","ProductPrice":8995,"CustomerId":"1e42e798-bdea-4de2-8f92-9766c358d471","OrderTime":"2021-01-24T20:18:20Z","CustomerFirstName":"Nefen","CustomerLastName":"Stode","CustomerEmail":"mantrag88@spiegel.de","CustomerPhone":"164-793-3398","CustomerAddress":"81 International Circle 34135 Florida - United States"}
96cf6a58-b771-4d5a-875d-80a7430266ac    {"OrderId":21601,"ProductId":"46dc2282-3bde-4f81-b16f-1d366de23f63","ProductName":"Max Perf 230","ProductBrand":"Oberbrunner, Dietrich and Hickle","ProductPrice":5995,"CustomerId":"96cf6a58-b771-4d5a-875d-80a7430266ac","OrderTime":"2021-01-24T20:15:00Z","CustomerFirstName":"Gilda","CustomerLastName":"Pagnin","CustomerEmail":"gmatthensen8c@jimdo.com","CustomerPhone":"740-358-3352","CustomerAddress":"81 Petterle Crossing 60630 Illinois - United States"}
d343e1a3-4d55-4756-94bc-7bbc0eddc9fd    {"OrderId":21604,"ProductId":"cc992519-6150-4570-8026-5fb2f1a430f5","ProductName":"Spectra5 Sidekick 595","ProductBrand":"Jones-Stokes","ProductPrice":7595,"CustomerId":"d343e1a3-4d55-4756-94bc-7bbc0eddc9fd","OrderTime":"2021-01-24T20:20:00Z","CustomerFirstName":"Theodoric","CustomerLastName":"Loosley","CustomerEmail":"dwoolnough5p@mashable.com","CustomerPhone":"200-994-6230","CustomerAddress":"8 Kenwood Center 11044 New York - United States"}
1f1d77d1-1a06-451b-8c00-0a47343c79bb    {"OrderId":21607,"ProductId":"81fb3fa0-cb65-4b32-9cbe-9080d1ee0566","ProductName":"Impreza Max 387","ProductBrand":"Williamson Group","ProductPrice":7197,"CustomerId":"1f1d77d1-1a06-451b-8c00-0a47343c79bb","OrderTime":"2021-01-24T20:25:00Z","CustomerFirstName":"Garrett","CustomerLastName":"Alp","CustomerEmail":"abachnicw@wikia.com","CustomerPhone":"860-560-3822","CustomerAddress":"60 Lien Avenue 94142 California - United States"}
531a2be5-2343-4742-996d-3c5c5af3be27    {"OrderId":21612,"ProductId":"4f33d6ef-f9c3-439a-afe9-bd87102460ad","ProductName":"Perf Spectra5 154","ProductBrand":"Langworth-Little","ProductPrice":14995,"CustomerId":"531a2be5-2343-4742-996d-3c5c5af3be27","OrderTime":"2021-01-24T20:33:20Z","CustomerFirstName":"Herschel","CustomerLastName":"Ryall","CustomerEmail":"pashfull1d@nytimes.com","CustomerPhone":"118-236-7205","CustomerAddress":"98329 Novick Lane 84403 Utah - United States"}
....
```

You can also check the AKHQ UI at http://localhost:8082 and check the content of the topic `orders-enriched`. 

Please check the input topics (aka orders, customers and products) to see their payload and understand the powerfull of the join operation.

# How to stop ? 

Quit the terminal via `CTRL+C` and stop all docker containers with :

``` bash
docker rm -f $(docker ps -aq) > /dev/null 2>&1
```