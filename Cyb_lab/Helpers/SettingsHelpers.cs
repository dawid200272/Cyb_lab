using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace Cyb_lab.Helpers;

public static class SettingsHelpers
{
	#region Other methods
	public static void AddOrUpdateAppSettingsSystemText<T>(string sectionPathKey, T value)
	{
		var serializerOptions = new JsonSerializerOptions()
		{
			WriteIndented = true,
		};

		var filePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");

		string json = File.ReadAllText(filePath);
		dynamic jsonObj = JsonSerializer.Deserialize<dynamic>(json)!;

		SetValueRecursively(sectionPathKey, jsonObj, value);

		string output = JsonSerializer.Serialize(jsonObj, serializerOptions);

		File.WriteAllText(filePath, output);
	}

	public static void SetValueRecursivelySystemText<T>(string sectionPathKey, dynamic jsonObj, T value)
	{
		var remainingSections = sectionPathKey.Split(":", 2);

		var currentSection = remainingSections[0];

		if (remainingSections.Length > 1)
		{
			var nextSection = remainingSections[1];
			SetValueRecursively(nextSection, jsonObj[currentSection], value);
		}
		else
		{
			jsonObj[currentSection] = value;
		}
	}

	//public static void AddOrUpdateRangeAppSettings<T>(IDictionary<string, T> propertiesToUpdate)
	//{
	//	var serializerOptions = new JsonSerializerOptions()
	//	{
	//		WriteIndented = true,
	//	};

	//	var filePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");

	//	string json = File.ReadAllText(filePath);
	//	dynamic jsonObj = JsonSerializer.Deserialize<dynamic>(json)!;

	//	foreach ( var property in propertiesToUpdate )
	//	{
	//		SetValueRecursively(property.Key, jsonObj, property.Value);
	//	}

	//	string output = JsonSerializer.Serialize(jsonObj, serializerOptions);

	//	File.WriteAllText(filePath, output);
	//}

	public static dynamic SetValueRangeRecursivelyInDynamicJson<T>(IDictionary<string, T> propertiesToUpdate, dynamic jsonObj)
	{
		foreach (var property in propertiesToUpdate)
		{
			SetValueRecursively(property.Key, jsonObj, property.Value);
		}

		return jsonObj;
	}

	//public static void AddOrUpdateRangeAppSettings<T>(IDictionary<string, T> propertiesToUpdate)
	//{
	//	var serializerOptions = new JsonSerializerOptions()
	//	{
	//		WriteIndented = true,
	//	};

	//	var filePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");

	//	string json = File.ReadAllText(filePath);
	//	dynamic jsonObj = JsonSerializer.Deserialize<dynamic>(json)!;

	//	foreach (var property in propertiesToUpdate)
	//	{
	//		SetValueRecursively(property.Key, jsonObj, property.Value);
	//	}

	//	string output = JsonSerializer.Serialize(jsonObj, serializerOptions);

	//	File.WriteAllText(filePath, output);
	//}

	public static dynamic GetDynamicJsonSystemText()
	{
		var filePath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");

		string json = File.ReadAllText(filePath);
		dynamic jsonObj = JsonSerializer.Deserialize<dynamic>(json)!;

		return jsonObj;
	}

	public static void WriteInAppSettingsSystemText(dynamic jsonObj)
	{
		var filePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");

		var serializerOptions = new JsonSerializerOptions()
		{
			WriteIndented = true,
		};

		string output = JsonSerializer.Serialize(jsonObj, serializerOptions);

		File.WriteAllText(filePath, output);
	}

	public static void AddOrUpdateAppSetting<T>(string sectionPathKey, T value)
	{
		try
		{
			var filePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
			string json = File.ReadAllText(filePath);
			dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

			SetValueRecursively(sectionPathKey, jsonObj, value);

			string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
			File.WriteAllText(filePath, output);

		}
		catch (Exception ex)
		{
			Console.WriteLine("Error writing app settings | {0}", ex.Message);
		}
	} 
	#endregion

	public static void SetValueRecursively<T>(string sectionPathKey, dynamic jsonObj, T value)
	{
		// split the string at the first ':' character
		var remainingSections = sectionPathKey.Split(":", 2);

		var currentSection = remainingSections[0];
		if (remainingSections.Length > 1)
		{
			// continue with the procress, moving down the tree
			var nextSection = remainingSections[1];
			SetValueRecursively(nextSection, jsonObj[currentSection], value);
		}
		else
		{
			// we've got to the end of the tree, set the value
			jsonObj[currentSection] = value;
		}
	}

	public static dynamic GetDynamicJson()
	{
		dynamic jsonObj = null;

		try
		{
			var filePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
			string json = File.ReadAllText(filePath);
			jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
		}
		catch (Exception ex)
		{
			Console.WriteLine("Error writing app settings | {0}", ex.Message);
		}

		return jsonObj;
	}

	public static void WriteInAppSettings(dynamic jsonObj)
	{
		try
		{
			var filePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");

			string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
			File.WriteAllText(filePath, output);

		}
		catch (Exception ex)
		{
			Console.WriteLine("Error writing app settings | {0}", ex.Message);
		}
	}
}
