namespace ImageProcessing.Stacking
{
    public class HeliconStacker
    {
        
        // private ImageSaver imageSaver = new ImageSaver(20);
        // private StoragePathResolver resolver;
        // private string lastSavedFilename = null;
        //
        // public HeliconStacker(StoragePathResolver resolver)
        // {
        //     this.resolver = resolver;
        //     imageSaver.Start();
        // }
        //
        // public void Start()
        // {
        //     imageSaver.Start();
        // }
        //
        // public void Stop()
        // {
        //     imageSaver.Stop();
        // }
        //
        // //public void Add(StackableImage image)
        // //{
        // //    image.Filename = resolver.RawFilePath();
        // //    imageSaver.Add(image);
        // //    lastSavedFilename = image.Filename;
        // //}
        //
        // public async Task<bool> StackAsync(IAcquisitionInfo state)
        // {
        //     //Wait for save
        //     while (!imageSaver.IsFinished())
        //     {
        //         await Task.Delay(100);
        //         Debug.Write("#");
        //     }
        //
        //     var source = Directory.GetParent(lastSavedFilename);
        //     var destination = resolver.StackedFilePath(SaveFormat.Jpeg);
        //
        //     string strCmdText;
        //     strCmdText = String.Format("\"C:\\Program Files\\Helicon Software\\Helicon Focus 6\\HeliconFocus.exe\" -silent \"{0}\" -rp:18 -sp:1 -mp:1 -save:\"{1}\"",
        //         source, destination);
        //     //Debug.WriteLine(strCmdText);
        //     Process cmd = new Process();
        //     cmd.StartInfo.FileName = "cmd.exe";
        //     cmd.StartInfo.RedirectStandardInput = true;
        //     cmd.StartInfo.RedirectStandardOutput = true;
        //     cmd.StartInfo.CreateNoWindow = true;
        //     cmd.StartInfo.UseShellExecute = false;
        //     cmd.Start();
        //     await cmd.StandardInput.WriteLineAsync(strCmdText);
        //     await cmd.StandardInput.FlushAsync();
        //     cmd.StandardInput.Close();
        //     cmd.WaitForExit();
        //
        //     //Wait until file is present
        //     bool foundFile = false;
        //     int count = 0;
        //     while (count < 10)
        //     {
        //         if (File.Exists(destination))
        //         {
        //             foundFile = true;
        //             break;
        //         }
        //         await Task.Delay(500);
        //         count++;
        //         Debug.Write("?");
        //     }
        //     return foundFile;
        // }
        //
        // public bool StackAsync()
        // {
        //     throw new NotImplementedException();
        // }
    }
}
