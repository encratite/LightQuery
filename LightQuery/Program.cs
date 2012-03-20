using System;
using System.Data.SQLite;

namespace LightQuery
{
	class Program
	{
		static void PrintQueryOutput(SQLiteDataReader reader)
		{
			while (reader.Read())
			{
				for (int i = 0; i < reader.FieldCount; i++)
				{
					object field = reader[i];
					if (field.GetType() == typeof(string))
						Console.Write("'{0}' ", field);
					else
						Console.Write("{0} ", field);
				}
				Console.WriteLine("");
			}
			if (reader.RecordsAffected > 0)
				Console.WriteLine("{0} records affected", reader.RecordsAffected);
		}

		static void PerformQuery(string query, SQLiteConnection connection)
		{
			try
			{
				using (SQLiteCommand command = new SQLiteCommand(query, connection))
				{
					using (SQLiteDataReader reader = command.ExecuteReader())
						PrintQueryOutput(reader);
				}
			}
			catch (SQLiteException exception)
			{
				Console.WriteLine(exception.Message);
			}
		}

		static void HandleDatabase(string path)
		{
			using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}", path)))
			{
				connection.Open();
				string buffer = "";
				while (true)
				{
					Console.Write("> ");
					buffer += Console.ReadLine();
					int offset = buffer.IndexOf(";");
					if (offset == -1)
						continue;
					string query = buffer.Substring(0, offset).Trim();
					buffer = buffer.Substring(offset + 1);
					PerformQuery(query, connection);
				}
			}
		}

		static void Main(string[] arguments)
		{
			if (arguments.Length != 1)
			{
				Console.WriteLine("You must specify a database to open");
				return;
			}
			string path = arguments[0];
			HandleDatabase(path);
		}
	}
}
