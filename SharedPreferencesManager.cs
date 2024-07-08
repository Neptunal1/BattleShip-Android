using Android.Content;
using Android.Preferences;
using Newtonsoft.Json;

public class SharedPreferencesManager
{
    // Save a 2D array to SharedPreferences
    public static void SaveArrayToSharedPreferences(Context context, string key, int[,] array)
    {
        ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(context);
        ISharedPreferencesEditor editor = prefs.Edit();
        string arrayJson = JsonConvert.SerializeObject(array);
        editor.PutString(key, arrayJson);
        editor.Apply();
    }

    // Retrieve a 2D array from SharedPreferences
    public static int[,] GetArrayFromSharedPreferences(Context context, string key)
    {
        ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(context);
        string arrayJson = prefs.GetString(key, null);
        if (arrayJson != null)
        {
            return JsonConvert.DeserializeObject<int[,]>(arrayJson);
        }
        else
        {
            // Handle case where array is not found in SharedPreferences
            return null;
        }
    }
}
