using System;
using System.IO;
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
		}

		static void PerformQuery(string query, SQLiteConnection connection, bool isManualQuery)
		{
			try
			{
				using (SQLiteCommand command = new SQLiteCommand(query, connection))
				{
					using (SQLiteDataReader reader = command.ExecuteReader())
					{
						PrintQueryOutput(reader);
						if (isManualQuery && reader.RecordsAffected > 0)
							Console.WriteLine("{0} records affected", reader.RecordsAffected);
					}
				}
			}
			catch (SQLiteException exception)
			{
				Console.WriteLine("Error in query: {0}", query);
				Console.WriteLine(exception.Message);
			}
		}

		static void HandleDatabase(string path, string[] lines = null)
		{
			bool isManualQuery = lines == null;
			const string comment = "--";
			using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}", path)))
			{
				connection.Open();
				string buffer = "";
				int lineIndex = 0;
				while (lines == null || lineIndex < lines.Length)
				{
					string line;
					if (isManualQuery)
					{
						Console.Write("> ");
						line = Console.ReadLine();
					}
					else
					{
						line = lines[lineIndex];
						lineIndex++;
					}
					line = line.Trim();
					//Skip comments
					if (line.Length >= comment.Length && line.Substring(0, comment.Length) == comment)
						continue;
					buffer += line;
					int offset = buffer.IndexOf(";");
					if (offset == -1)
						continue;
					string query = buffer.Substring(0, offset).Trim();
					buffer = buffer.Substring(offset + 1);
					PerformQuery(query, connection, isManualQuery);
				}
			}
		}

		static void Main(string[] arguments)
		{
			if (arguments.Length < 1 || arguments.Length > 2)
			{
				string application = Environment.GetCommandLineArgs()[0];
				Console.WriteLine("{0} <database>", application);
				Console.WriteLine("{0} <database> <query script>", application);
				return;
			}
			string databasePath = arguments[0];
			if (arguments.Length == 1)
				HandleDatabase(databasePath);
			else
			{
				string scriptPath = arguments[1];
				try
				{
					string[] lines = File.ReadAllLines(scriptPath);
					HandleDatabase(databasePath, lines);
				}
				catch (IOException exception)
				{
					Console.WriteLine(exception.Message);
				}
			}
		}
	}
}
