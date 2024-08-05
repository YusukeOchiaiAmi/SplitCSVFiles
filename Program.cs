class Program
{
    static async Task Main()
    {
        DirectoryInfo dirInfo = new DirectoryInfo("CSV");
        FileInfo[] csvFiles = dirInfo.GetFiles("*.csv");

        if (csvFiles.Length == 0)
        {
            Console.WriteLine("CSV files cloud not found in CSV folder.");
        }
        else
        {
            foreach (FileInfo csvFile in csvFiles)
            {
                Console.WriteLine(
                    "Do you want to delete the original files after splitting? [y/N]"
                );
                string? answer = Console.ReadLine();
                bool deleteFlag = false;
                if (answer?.ToLower() == "y")
                {
                    deleteFlag = true;
                }
                Console.WriteLine($"Processing Split [./CSV/{csvFile.Name}] started...");
                await SplitCsvFile(csvFile.Name, deleteFlag);
            }
        }
    }

    static async Task SplitCsvFile(string inputFileName, bool deleteFlag)
    {
        //1GBごとに分割
        float splitSizeInGB = 1.0f;
        //0.15GBほど余裕を持たせる
        float checkSizeInGB = splitSizeInGB - 0.15f;
        float gigabyte = 1024f * 1024f * 1024f;

        string csvFileName = Path.GetFileNameWithoutExtension(inputFileName);
        string inputFilePath = $"./CSV/{csvFileName}.csv";
        int outputFileNameSuffix = 1;
        string outputFileName = $"{csvFileName}_split_{outputFileNameSuffix}.csv";
        string timeStamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        string outputDirectory = $"./SPLIT/{timeStamp}";
        Directory.CreateDirectory(outputDirectory);
        string outputFilePath = outputDirectory + '/' + outputFileName;

        try
        {
            //10万行ごとにチェック
            int checkCount = 100 * 1000;
            Console.WriteLine($"Spliting to [{outputFilePath}]");

            int count = 0;
            StreamWriter writer = new StreamWriter(outputFilePath, true);
            using (StreamReader reader = new StreamReader(inputFilePath))
            {
                var header = reader.ReadLine();
                writer.WriteLine(header);
                while (true)
                {
                    count++;
                    var line = await reader.ReadLineAsync();
                    if (line is null)
                    {
                        break;
                    }
                    await writer.WriteLineAsync(line);
                    if (count == checkCount)
                    {
                        var outputFileInfo = new FileInfo(outputFilePath);
                        float fileSizeInGB = outputFileInfo.Length / gigabyte;
                        if (fileSizeInGB >= checkSizeInGB)
                        {
                            writer.Close();
                            outputFileNameSuffix++;
                            outputFileName = $"{csvFileName}_split_{outputFileNameSuffix}.csv";
                            outputFilePath = $"./SPLIT/{timeStamp}/{outputFileName}";
                            Console.WriteLine($"Spliting to [{outputFilePath}]");
                            writer = new StreamWriter(outputFilePath, true);
                            writer.WriteLine(header);
                        }
                        count = 0;
                    }
                }
            }
            Console.WriteLine($"Processing Split [{inputFilePath}] completed...");
            if (deleteFlag)
            {
                File.Delete(inputFilePath);
                Console.WriteLine($"File [{inputFilePath}] has been deleted.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"An error occurred when processing [InputFilePath : {inputFilePath}] [OutputFilePath : {outputFilePath}]"
            );
            Console.WriteLine("Error Message : " + ex.Message);
        }
        return;
    }
}
