using PCLStorage;
using Plugin.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SaveBinaryFilesInXamarin
{
    
    public partial class MainPage : ContentPage
    {
        
        byte[] b;
        public MainPage()
        {
            InitializeComponent();
            startCrossMedia();
        }
        public async void startCrossMedia()
        {
            await CrossMedia.Current.Initialize();

        }
      
        private async void TakePhoto_ClickedAsync(object sender, EventArgs e)
        {
            if (!CrossMedia.Current.IsCameraAvailable)
            {
                await DisplayAlert("No Camera", "No camera available.", "OK");
                return;
            }

            var file1 = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                SaveToAlbum = false
            });
            if (file1 == null)
            {
                return;
            }
            imagefromLibraryorCamera.Source = ImageSource.FromStream(() => file1.GetStream());
            byte[] b1 = new byte[file1.GetStream().Length];
            await file1.GetStream().ReadAsync(b1, 0, (int)b1.Length);
            b = b1;
        }

        private async void FromLibrary_ClickedAsync(object sender, EventArgs e)
        {

            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                await DisplayAlert("Error", "Can not take an image from device Library", "Ok");
                return;
            }
            var file2 = await CrossMedia.Current.PickPhotoAsync();

            if (file2 == null)
            {
                return;
            }
            imagefromLibraryorCamera.Source = ImageSource.FromStream(() => file2.GetStream());
            byte[] b2 = new byte[file2.GetStream().Length];
            await file2.GetStream().ReadAsync(b2, 0, (int)b2.Length);
            b = b2;
        }

        private async void SaveButton_ClickedAsync(object sender, EventArgs e)
        {
            //creates folder where files will be stored

            IFileSystem fileSystem = FileSystem.Current;
            IFolder rootFolder = fileSystem.LocalStorage;
            IFolder photosFolder = await rootFolder.CreateFolderAsync("Photos", PCLStorage.CreationCollisionOption.OpenIfExists);
            IFile saveFile = await photosFolder.CreateFileAsync("Name", PCLStorage.CreationCollisionOption.ReplaceExisting);
            
            if (b!=null)
            {
                //if we want to save binary data as text file wee need to convert it to Base64String because some values are imposible to save in UFT-8
                string imageAsBytes = System.Convert.ToBase64String(b);
                //now we can save file as it was a text file
                await saveFile.WriteAllTextAsync(imageAsBytes);
            }
            else
            {
                await DisplayAlert("No photo selected", "First you need to choose the image to save", "Ok");
                return;
            }
            folderPath.Text = photosFolder.Path;
        }

        private async void LoadButtom_ClickedAsync(object sender, EventArgs e)
        {
            //we need to get back to folder where we saved the image

            IFileSystem fileSystem = FileSystem.Current;
            IFolder rootFolder = fileSystem.LocalStorage;
            IFolder photosFolder = await rootFolder.GetFolderAsync("Photos");
            //It is just an example more advanced projects need to have some JSON or txt file with all the folders,folder paths and file names saved
            IFile loadFile= await photosFolder.GetFileAsync("Name");
            string loadedBinaryImage = await loadFile.ReadAllTextAsync();
            //now we need to reverse conversion of the file to get the bytes array
            byte[] fromTextToArray = System.Convert.FromBase64String(loadedBinaryImage);
            loadedImage.Source = ImageSource.FromStream(() => new MemoryStream(fromTextToArray));
        }
    }
}
