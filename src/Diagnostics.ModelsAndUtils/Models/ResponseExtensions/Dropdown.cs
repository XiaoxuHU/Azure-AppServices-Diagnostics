﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Diagnostics.ModelsAndUtils.Models.ResponseExtensions
{
    public class Dropdown
    {
        /// <summary>
        /// Represents the label shown for dropdown selector
        /// </summary>
        public string Label;

        /// <summary>
        /// Tuple representing Data.
        /// First Item represents item in Dropdown, Second Item represents whether selected by default in dropdwon, Third Item represents the body after dropdown selection
        /// </summary>
        public List<Tuple<string, bool, Response>> Data;

        /// <summary>
        /// Creates an instance of Dropdown Class
        /// </summary>
        /// <param name="label">Dropdown Label</param>
        /// <param name="data">Dropdown Data</param>
        public Dropdown(string label, List<Tuple<string, bool, Response>> data)
        {
            this.Label = label;
            this.Data = data;
        }
    }


    public enum DropdownType
    {
        Legacy,
        Fabric
    }

    public enum DropdownPosition
    {
        FloatLeft,
        FloatRight
    }

    public static class ResponseDropdownExtension
    {
        /// <summary>
        /// Adds a Dropdown View to Response
        /// </summary>
        /// <param name="response">Response</param>
        /// <param name="dropdownView">Dropdown ViewModel</param>
        /// <param name="title">Title</param>
        /// <returns></returns>
        /// <example>
        /// This sample shows how to use <see cref="AddDropdownView"/> method.
        /// <code>
        /// public async static Task<![CDATA[<Response>]]> Run(DataProviders dp, OperationContext cxt, Response res)
        /// {
        ///      string label = "select item here";
        ///      List<![CDATA[<Tuple<string, bool, Response>>]]> data = new List<![CDATA[<Tuple<string, bool, Response>>]]>();
        ///
        ///      string firstDataKey = "key1";
        ///      bool selected = true;
        ///      var firstDataEntry = new Response();
        ///      firstDataEntry.AddMarkdownView(@"some markdown content");
        ///
        ///     data.Add(new Tuple<![CDATA[<string, bool, Response>]]>(firstDataKey, selected, firstDataEntry));
        ///
        ///     Dropdown dropdownViewModel = new Dropdown(label, data);
        ///     res.AddDropdownView(dropdownViewModel,"Title");
        /// }
        /// </code>
        /// </example>
        public static DiagnosticData AddDropdownView(this Response response, Dropdown dropdownView, string title = null)
        {
            return AddDropdownView(response, dropdownView, title, DropdownType.Legacy, DropdownPosition.FloatLeft);
        }

        /// <summary>
        /// Adds a Dropdown View to Response
        /// </summary>
        /// <param name="response">Response</param>
        /// <param name="dropdownView">Dropdown View</param>
        /// <param name="title">Title</param>
        /// <param name="type">Dropdown Type</param>
        /// <param name="position">Dropdown Position</param>
        /// <returns></returns>
        /// <example>
        /// This sample shows how to use <see cref="AddDropdownView"/> method.
        /// <code>
        /// public async static Task<![CDATA[<Response>]]> Run(DataProviders dp, OperationContext cxt, Response res)
        /// {
        ///      string label = "select item here";
        ///      List<![CDATA[<Tuple<string, bool, Response>>]]> data = new List<![CDATA[<Tuple<string, bool, Response>>]]>();
        ///
        ///      string firstDataKey = "key1";
        ///      bool selected = true;
        ///      var firstDataEntry = new Response();
        ///      firstDataEntry.AddMarkdownView(@"some markdown content");
        ///
        ///     data.Add(new Tuple<![CDATA[<string, bool, Response>]]>(firstDataKey, selected, firstDataEntry));
        ///
        ///     Dropdown dropdownViewModel = new Dropdown("label", data);
        ///     res.AddDropdownView(dropdownViewModel,"Title",DropdownType.Fabric,DropdownPosition.FloatLeft);
        /// }
        /// </code>
        /// </example>
        public static DiagnosticData AddDropdownView(this Response response, Dropdown dropdownView, string title, DropdownType type = DropdownType.Legacy, DropdownPosition position = DropdownPosition.FloatLeft)
        {
            var table = new DataTable();
            table.Columns.Add(new DataColumn("Label", typeof(string)));
            table.Columns.Add(new DataColumn("Key", typeof(string)));
            table.Columns.Add(new DataColumn("Selected", typeof(bool)));
            table.Columns.Add(new DataColumn("Value", typeof(string)));
            table.Columns.Add(new DataColumn("DropdownType", typeof(string)));
            table.Columns.Add(new DataColumn("DropdownPosition", typeof(string)));

            foreach (var item in dropdownView.Data)
            {
                List<DiagnosticDataApiResponse> dataSet = item.Item3.Dataset.Select(entry =>
                    new DiagnosticDataApiResponse()
                    {
                        RenderingProperties = entry.RenderingProperties,
                        Table = entry.Table.ToDataTableResponseObject()
                    }).ToList();

                table.Rows.Add(new object[] {
                    dropdownView.Label,
                    item.Item1,
                    item.Item2,
                    JsonConvert.SerializeObject(dataSet, new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    }),
                    type,
                    position
                });
            }


            var diagData = new DiagnosticData()
            {
                Table = table,
                RenderingProperties = new Rendering(RenderingType.DropDown)
                {
                    Title = title ?? string.Empty
                }
            };

            response.Dataset.Add(diagData);
            return diagData;
        }
    }
}
