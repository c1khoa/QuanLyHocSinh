using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Configuration;

namespace QuanLyHocSinh.Model.Entities
{ 
	public abstract class BaseDAL
	{
		protected string ConnectionString { get; private set; }

		public BaseDAL()
		{
			// Lấy chuỗi kết nối từ App.config
			ConnectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
		}

		protected MySqlConnection GetConnection()
		{
			return new MySqlConnection(ConnectionString);
		}
	}
}
