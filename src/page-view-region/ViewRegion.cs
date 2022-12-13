namespace page_view_region
{
    public class ViewRegion
	{
		private string _page;
		private string _user;
		private string _region;
		
		public string page
		{
			get
			{
				return this._page;
			}
			set
			{
				this._page = value;
			}
		}
		public string user
		{
			get
			{
				return this._user;
			}
			set
			{
				this._user = value;
			}
		}
		public string region
		{
			get
			{
				return this._region;
			}
			set
			{
				this._region = value;
			}
		}
	}
}
