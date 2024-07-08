using Android;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Widget;
using Firebase;
using Firebase.Database;
using Android.Support.V4.App;
using Java.IO;
using Android.Graphics;
using Environment = Android.OS.Environment;
using Uri = Android.Net.Uri;
using System;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Android.Content.PM;
using Android.Icu.Text;
using System.IO;

namespace BattleShip
{
    [Activity(Label = "profiles")]
    public class profiles : Activity, IValueEventListener
    {
        private static readonly int REQUEST_IMAGE_CAPTURE = 1;
        private static readonly int REQUEST_PERMISSIONS = 2;

        private ImageView profileImage;
        private TextView nameText, winsText, lossesText;
        private DatabaseReference databaseReference;

        private string userName;
        private int wins = 0, losses = 0;
        private string currentPhoto = "";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.profile);

            nameText = FindViewById<TextView>(Resource.Id.tname);
            winsText = FindViewById<TextView>(Resource.Id.twins);
            lossesText = FindViewById<TextView>(Resource.Id.tlosses);
            profileImage = FindViewById<ImageView>(Resource.Id.timage);

            Button picBtn = FindViewById<Button>(Resource.Id.picbtn);
            picBtn.Click += TakePicture;

            FirebaseApp.InitializeApp(Application.Context);
            databaseReference = FirebaseDatabase.Instance.Reference;

            var prefs = Application.Context.GetSharedPreferences("BattleShipPrefs", FileCreationMode.Private);
            userName = prefs.GetString("username", null);
            wins = prefs.GetInt("wins", 0);                 // 0 is the default value if the key is not found
            losses = prefs.GetInt("losses", 0);            // 0 is the default value if the key is not found

            DatabaseReference userRef = databaseReference.Child("users").Child(userName);
            userRef.AddValueEventListener(this);

            RequestPermissions();
        }

        private void RequestPermissions()
        {
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.Camera) != (int)Permission.Granted ||
                ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) != (int)Permission.Granted ||
                ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage) != (int)Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new string[]
                {
                    Manifest.Permission.Camera,
                    Manifest.Permission.WriteExternalStorage,
                    Manifest.Permission.ReadExternalStorage
                }, REQUEST_PERMISSIONS);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            if (requestCode == REQUEST_PERMISSIONS)
            {
                if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                {
                    Toast.MakeText(this, "Permissions granted", ToastLength.Short).Show();
                }
                else
                {
                    Toast.MakeText(this, "Permissions denied", ToastLength.Short).Show();
                }
            }
        }

        private void TakePicture(object sender, System.EventArgs e)
        {
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.Camera) == (int)Permission.Granted &&
                ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) == (int)Permission.Granted)
            {
                Intent takePictureIntent = new Intent(MediaStore.ActionImageCapture);
                if (takePictureIntent.ResolveActivity(PackageManager) != null)
                {
                    Java.IO.File photoFile = null;
                    try
                    {
                        photoFile = CreateImageFile();
                    }
                    catch (Java.IO.IOException ex)
                    {
                        // Handle error
                    }
                    if (photoFile != null)
                    {
                        Uri photoURI = FileProvider.GetUriForFile(this, "com.companyname.battleship.fileprovider", photoFile);
                        takePictureIntent.PutExtra(MediaStore.ExtraOutput, photoURI);
                        StartActivityForResult(takePictureIntent, REQUEST_IMAGE_CAPTURE);
                    }
                }
            }
            else
            {
                Toast.MakeText(this, "Camera and Storage permissions are required to take pictures", ToastLength.Short).Show();
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == REQUEST_IMAGE_CAPTURE && resultCode == Result.Ok)
            {
                Bitmap bitmap = BitmapFactory.DecodeFile(currentPhoto);
                profileImage.SetImageBitmap(bitmap);
                byte[] imageData = HelperFile.BitmapToByteArray(bitmap); // Convert the bitmap to a byte array
                UploadImageToDatabase(bitmap); // Upload the byte array to Firebase 
            }
        }

        public static byte[] BitmapToByteArray(Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Compress(Bitmap.CompressFormat.Png, 0, stream);
                return stream.ToArray();
            }
        }

        public void UploadImageToDatabase(Bitmap bitmap)
        {
            MemoryStream stream = new MemoryStream(); //making shoreter string (lower quailty) 
            bitmap.Compress(Bitmap.CompressFormat.Jpeg, 50, stream);                            // Adjust quality as needed (0-100)
            byte[] imageData = stream.ToArray();                                                 // Convert compressed bitmap to byte array
            string base64Image = Convert.ToBase64String(imageData);                              // Convert byte array to Base64 string
            FirebaseDatabase.Instance.GetReference($"users/{userName}/image").SetValue(base64Image); // Set Base64 string value to Firebase database reference
        }

        private Java.IO.File CreateImageFile()
        {
            // Create an image file name
            string timeStamp = new SimpleDateFormat("yyyyMMdd_HHmmss").Format(new Java.Util.Date());
            string imageFileName = "JPEG_" + timeStamp + "_";
            Java.IO.File storageDir = GetExternalFilesDir(Environment.DirectoryPictures);
            Java.IO.File image = Java.IO.File.CreateTempFile(imageFileName,  /* prefix */ ".jpg",/* suffix */ storageDir/* directory */);
            currentPhoto = image.AbsolutePath; // Save a file: path for use with ACTION_VIEW intents
            return image;
        }

        public void OnDataChange(DataSnapshot snapshot)
        {
            if (snapshot.Exists())
            {
                // Update UI with profile information
                RunOnUiThread(() =>
                {
                    nameText.Text = "Name: " + userName;
                    winsText.Text = "Wins: " + wins.ToString() + "  ";
                    lossesText.Text = "Losses: " + losses.ToString();

                    string imageString = snapshot.Child("image").Value?.ToString(); // Get the image string from Firebase
                    byte[] imageData = Convert.FromBase64String(imageString); // Convert the Base64 string back to byte array
                    Bitmap bitmap = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length); // Convert the byte array to Bitmap
                    profileImage.SetImageBitmap(bitmap);

                });
            }
        }

        public void OnCancelled(DatabaseError error)
        {
            // Handle any errors
            RunOnUiThread(() =>
            {
                Toast.MakeText(this, "Error: " + error.Message, ToastLength.Short).Show();
            });
        }
  
    }
}
