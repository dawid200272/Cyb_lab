using Newtonsoft.Json;

namespace Cyb_lab.Helpers;

public static class SettingsHelpers
{
	public static string CurrentDirectory = Directory.GetCurrentDirectory();

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
			var filePath = Path.Combine(CurrentDirectory, "appsettings.json");
			string json = File.ReadAllText(filePath);
			jsonObj = JsonConvert.DeserializeObject(json);
		}
		catch (Exception ex)
		{
			Console.WriteLine("Error reading app settings | {0}", ex.Message);
		}

		return jsonObj;
	}

	public static void WriteInAppSettings(dynamic jsonObj)
	{
		try
		{
			var filePath = Path.Combine(CurrentDirectory, "appsettings.json");

			string output = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
			File.WriteAllText(filePath, output);

		}
		catch (Exception ex)
		{
			Console.WriteLine("Error writing app settings | {0}", ex.Message);
		}
	}
}
