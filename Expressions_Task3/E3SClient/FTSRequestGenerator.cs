﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Expressions_Task3.E3SClient
{
	public class FTSRequestGenerator
	{
		private readonly UriTemplate FTSSearchTemplate = new UriTemplate(@"data/searchFts?metaType={metaType}&query={query}&fields={fields}");
		private readonly Uri BaseAddress;

		public FTSRequestGenerator(string baseAddres) : this(new Uri(baseAddres))
		{
		}

		public FTSRequestGenerator(Uri baseAddress)
		{
			BaseAddress = baseAddress;
		}

		public Uri GenerateRequestUrl<T>(List<string> query = null, int start = 0, int limit = 10)
		{
			return GenerateRequestUrl(typeof(T), query, start, limit);
		}

		public Uri GenerateRequestUrl(Type type, List<string> query = null, int start = 0, int limit = 10)
		{
			string metaTypeName = GetMetaTypeName(type);

		    if (query == null)
		    {
		        query = new List<string>{"*"};
		    }

			var ftsQueryRequest = new FTSQueryRequest
			{
				Statements = query.Select(s=>new Statement{Query = s}).ToList(),
				Start = start,
				Limit = limit
			};

			var ftsQueryRequestString = JsonConvert.SerializeObject(ftsQueryRequest);

			var uri = FTSSearchTemplate.BindByName(BaseAddress,
				new Dictionary<string, string>()
				{
					{ "metaType", metaTypeName },
					{ "query", ftsQueryRequestString }
				});

			return uri;
		}

		private string GetMetaTypeName(Type type)
		{
			var attributes = type.GetCustomAttributes(typeof(E3SMetaTypeAttribute), false);

			if (attributes.Length == 0)
				throw new Exception(string.Format("Entity {0} do not have attribute E3SMetaType", type.FullName));

			return ((E3SMetaTypeAttribute)attributes[0]).Name;
		}

	}
}