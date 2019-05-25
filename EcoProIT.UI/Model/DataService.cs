﻿using System;

namespace EcoProIT.UI.Model
{
    public class DataService : IDataService
    {
        public void GetData(Action<DataItem, Exception> callback)
        {
            // Use this to connect to the actual data service

            var item = new DataItem("test");
            callback(item, null);
        }
    }
}