﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace Expressions_Task3.E3SClient.Entities
{
	public class Skills
	{
		[JsonProperty]
		public string nativespeaker { get; set; }

		[JsonProperty]
		public string expert { get; set; }

		[JsonProperty]
		public string advanced { get; set; }

		[JsonProperty]
		public string intermediate { get; set; }

		[JsonProperty]
		public string novice { get; set; }

		[JsonProperty]
		public string position { get; set; }

		[JsonProperty]
		public string os { get; set; }

		[JsonProperty]
		public string db { get; set; }

		[JsonProperty]
		public string platform { get; set; }

		[JsonProperty]
		public string industry { get; set; }

		[JsonProperty]
		public string proglang { get; set; }

		[JsonProperty]
		public string language { get; set; }

		[JsonProperty]
		public string other { get; set; }

		[JsonProperty]
		public string primary { get; set; }

		[JsonProperty]
		public string category { get; set; }
	}

	public class WorkHistory
	{
		[JsonProperty]
		public string terms { get; set; }

		[JsonProperty]
		public string company { get; set; }

		[JsonProperty]
		public string companyurl { get; set; }

		[JsonProperty]
		public string position { get; set; }

		[JsonProperty]
		public string role { get; set; }

		[JsonProperty]
		public string project { get; set; }

		[JsonProperty]
		public string participation { get; set; }

		[JsonProperty]
		public string team { get; set; }

		[JsonProperty]
		public string database { get; set; }

		[JsonProperty]
		public string tools { get; set; }

		[JsonProperty]
		public string technologies { get; set; }

		[JsonProperty]
		public string startdate { get; set; }

		[JsonProperty]
		public string enddate { get; set; }

		[JsonProperty]
		public bool isepam { get; set; }

		[JsonProperty]
		public string epamproject { get; set; }
	}

	public class Recognition
	{
		[JsonProperty]
		public string nomination { get; set; }

		[JsonProperty]
		public string description { get; set; }

		[JsonProperty]
		public string recognitiondate { get; set; }

		[JsonProperty]
		public string points { get; set; }
	}

    public class Badge
    {
        [JsonProperty]
        public int id { get; set; }

        [JsonProperty]
        public int image { get; set; }

        [JsonProperty]
        public string date { get; set; }

        [JsonProperty]
        public string name { get; set; }

        [JsonProperty]
        public string category { get; set; }

        [JsonProperty]
        public bool e3sImageExists { get; set; }
    }

    public class Social
    {
        [JsonProperty]
        public string origin { get; set; }

        [JsonProperty]
        public string id { get; set; }

        [JsonProperty]
        public string status { get; set; }

        [JsonProperty]
        public string url { get; set; }

        [JsonProperty]
        public string visibility { get; set; }
    }

    [E3SMetaType("meta:people-suite:people-api:com.epam.e3s.app.people.api.data.pluggable.EmployeeEntity")]
	public class EmployeeEntity : E3SEntity
	{
		[JsonProperty]
		public double entityBoost { get; set; }

		[JsonProperty]
		public double documentBoost { get; set; }

		[JsonProperty]
		public List<string> phone;

		[JsonProperty]
		public Skills skill { get; set; }

		[JsonProperty]
		public string firstname { get; set; }

		[JsonProperty]
		public string lastname { get; set; }

		[JsonProperty]
		public List<string> fullname { get; set; }

		[JsonProperty]
		public List<string> country { get; set; }

		[JsonProperty]
		public List<string> city { get; set; }

		[JsonProperty]
		public List<string> email { get; set; }

		[JsonProperty]
		public List<string> skype { get; set; }

		[JsonProperty]
		public Dictionary<string, List<Social>> social { get; set; }

		[JsonProperty]
		public List<string> attachment { get; set; }

		[JsonProperty]
		public string manager { get; set; }

		[JsonProperty]
		public string superior { get; set; }

		[JsonProperty]
		public string startworkdate { get; set; }

		[JsonProperty]
		public string project { get; set; }

		[JsonProperty]
		public string projectall { get; set; }

		[JsonProperty]
		public List<string> trainer { get; set; }

		[JsonProperty]
		public List<string> kb { get; set; }

		[JsonProperty]
		public string certificate { get; set; }

		[JsonProperty]
		public string unit { get; set; }

		[JsonProperty]
		public string office { get; set; }

		[JsonProperty]
		public string room { get; set; }

		[JsonProperty]
		public string status { get; set; }

		[JsonProperty]
		public string car { get; set; }

		[JsonProperty]
		public string birthday { get; set; }

		[JsonProperty]
		public List<WorkHistory> workhistory { get; set; }


		[JsonProperty]
		public List<string> jobfunction { get; set; }

		[JsonProperty]
		public List<Recognition> recognition { get; set; }

		[JsonProperty]
		public List<Badge> badge { get; set; }

		[JsonProperty]
		public string dismissal { get; set; }

		[JsonProperty]
		public string endProbationDate { get; set; }

		[JsonProperty]
		public string endworkdate { get; set; }

		[JsonProperty]
		public string errupdatedate { get; set; }

		[JsonProperty]
		public string edulevel { get; set; }

		[JsonProperty]
		public string eduschool { get; set; }

		[JsonProperty]
		public string edufield { get; set; }

		[JsonProperty]
		public string eduendyear { get; set; }

		[JsonProperty]
		public string workstation { get; set; }

		[JsonProperty]
		public string nativename { get; set; }

		[JsonProperty]
		public string governmentalid { get; set; }

		[JsonProperty]
		public double billable { get; set; }

		[JsonProperty]
		public double nonbillable { get; set; }
	}
}
