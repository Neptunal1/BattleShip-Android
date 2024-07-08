using Android.Content;
using Android.Graphics;
using Android.Widget;
using System.Collections.Generic;
using System.IO;

namespace BattleShip
{
    public class HelperFile
    {
        public static void WriteFileToInternalStorage(Context context, Bitmap bitmap, string filename)
        {
            Android.Content.ISharedPreferences sp = context.GetSharedPreferences("details", FileCreationMode.Private);
            int counter = sp.GetInt("counter", 0);

            using (Stream stream = context.OpenFileOutput(filename + counter, FileCreationMode.Private))
            {
                try
                {
                    bitmap.Compress(Bitmap.CompressFormat.Png, 90, stream);
                    stream.Close();
                    counter++;
                    var editor = sp.Edit();
                    editor.PutInt("counter", counter);
                    editor.Commit();
                    Toast.MakeText(context, "Image saved", ToastLength.Long).Show();
                }
                catch (Java.IO.IOException e)
                {
                    e.PrintStackTrace();
                }
            }
        }

        public static Bitmap ReadFileFromInternalStorage(Context context, string filename)
        {
            Bitmap bitmap = null;

            try
            {
                using (Stream inputStream = context.OpenFileInput(filename))
                {
                    try
                    {
                        bitmap = BitmapFactory.DecodeStream(inputStream);
                    }
                    catch (Java.IO.IOException e)
                    {
                        e.PrintStackTrace();
                    }
                }
            }
            catch (Java.IO.IOException e)
            {
                Toast.MakeText(context, "File not found", ToastLength.Long).Show();
            }

            return bitmap;
        }

        public static List<Bitmap> GetAllFiles(Context context)
        {
            Java.IO.File directory = context.FilesDir;
            List<Bitmap> bitmaps = new List<Bitmap>();

            foreach (string file in directory.List())
            {
                Toast.MakeText(context, file, ToastLength.Long).Show();
                Bitmap bitmap = ReadFileFromInternalStorage(context, file);
                bitmaps.Add(bitmap);
            }

            return bitmaps;
        }

        public static byte[] BitmapToByteArray(Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Compress(Bitmap.CompressFormat.Png, 0, stream);
                return stream.ToArray();
            }
        }
    
    }

}