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
        public int currentNumber;
        public IFolder photosFolder;
        public byte[] b;
        public MainPage()
        {
            InitializeComponent();           
        }
        
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            //creates folder where files will be stored
            await CrossMedia.Current.Initialize();

            IFileSystem fileSystem = FileSystem.Current;
            IFolder rootFolder = fileSystem.LocalStorage;
            IFolder photosFolder = await rootFolder.CreateFolderAsync("Photos", PCLStorage.CreationCollisionOption.OpenIfExists);
                      
        }
      
        private async Task TakePhoto_ClickedAsync(object sender, EventArgs e)
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
            imagefromLibraryorCamera.Source = ImageSource.FromStream(() => file1.GetStream());
            byte[] b = new byte[file1.GetStream().Length];
            await file1.GetStream().ReadAsync(b, 0, (int)b.Length);
            currentNumber = 1;
        }

        private async Task FromLibrary_ClickedAsync(object sender, EventArgs e)
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
            byte[] b = new byte[file2.GetStream().Length];
            await file2.GetStream().ReadAsync(b, 0, (int)b.Length);
            currentNumber = 2;
        }

        private async Task SaveButton_ClickedAsync(object sender, EventArgs e)
        {
            IFile saveFile = await photosFolder.CreateFileAsync("Name", PCLStorage.CreationCollisionOption.ReplaceExisting);
            
            if (b.Length!=0)
            {
                //if we want to save binary data as text file wee need to convert it to Base64String because some values are imposible to save in UFT-8 etc.
                string imageAsBytes = System.Convert.ToBase64String(b);
                //now we can save file as it was a text file
                await saveFile.WriteAllTextAsync(imageAsBytes);
            }
            else
            {
                await DisplayAlert("No photo selected", "First you need to choose the image to save", "Ok");
                return;
            }
            
        }

        private async Task LoadButtom_ClickedAsync(object sender, EventArgs e)
        {
            //It is just an example more advanced projects need to have some JSON or txt file with all the folders, paths and file names saved
            IFile loadFile= await photosFolder.GetFileAsync("Name");
            string loadedBinaryImage = await loadFile.ReadAllTextAsync();
            //now we need to reverse file from string to bytes
            byte[] fromTextToArray = System.Convert.FromBase64String(loadedBinaryImage);
            loadedImage.Source = ImageSource.FromStream(() => new MemoryStream(fromTextToArray));
        }
    }
}
