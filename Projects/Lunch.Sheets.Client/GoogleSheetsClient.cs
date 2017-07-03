﻿using Lunch.Domain;
using Lunch.Domain.Config;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Lunch.Domain.ErrorHandling;

namespace Lunch.Sheets.Client
{
    public class GoogleSheetsClient: ISheetsClient
    {
        private static readonly HttpClient HttpClient = new HttpClient();
        
        public GoogleSheetsClient()
        {
            HttpClient.BaseAddress = new Uri("https://sheets.googleapis.com/v4/spreadsheets/");
        }

        public void SetAuthorization(string token)
        {
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<SheetsResponse> GetSheetData(string x, string y, string sheetId, string sheetName, SheetsClientConfig config = null)
        {
            var requestUrl = ApplyConfig($"{sheetId}/values/{sheetName}!{x}:{y}", config);
            
            var httpResult = await HttpClient.GetAsync(requestUrl).ConfigureAwait(false);
            if (!httpResult.IsSuccessStatusCode)
            {
                throw new ApiException("Failed to retrieve data of providers.");
            }
            var result = await MapSheetsResponse(httpResult.Content);
            return result;
        }

        public async Task<List<string>> GetSheetNames(string sheetId)
        {
            var httpResult = await HttpClient.GetAsync($"{sheetId}?fields=sheets.properties").ConfigureAwait(false);
            if (!httpResult.IsSuccessStatusCode)
            {
                throw new ApiException("Failed to retrieve data of providers.");
            }

            var result = await MapSheetsNamesResponse(httpResult.Content).ConfigureAwait(false);
            
            return result;
        }

        private string ApplyConfig(string url, SheetsClientConfig config)
        {
            var builder = new StringBuilder(url);

            if (config != null)
            {
                if (config.MajorDimension == MajorDimension.Columns)
                {
                    builder.Append("?majorDimension=columns");
                } 
            }

            return builder.ToString();
        }

        private async Task<SheetsResponse> MapSheetsResponse(HttpContent content)
        {
            var jObject = Newtonsoft.Json.Linq.JObject.Parse(await content.ReadAsStringAsync().ConfigureAwait(false));
            SheetsResponse result = new SheetsResponse();
            foreach (var row in jObject["values"])
            {
                List<string> rowValues = new List<string>();
                foreach (var value in row)
                {
                    rowValues.Add(value.ToString());
                }
                while (rowValues.Count < 2)
                {
                    rowValues.Add("");
                }
                result.Rows.Add(rowValues);
            }

            return result;
        }

        private async Task<List<string>> MapSheetsNamesResponse(HttpContent content)
        {
            var jObject = Newtonsoft.Json.Linq.JObject.Parse(await content.ReadAsStringAsync().ConfigureAwait(false));
            var result = new List<string>();
            foreach (var property in jObject["sheets"])
            {
                result.Add(property["properties"]["title"].ToString());
            }

            return result;
        }
    }
}