namespace ratio_list_converter.Parser;

public record ConvertedFile(MimeType MimeType, string FileName, string? Content, byte[]? Stream)
{
    public static ConvertedFile FromCsv(string fileName, string content)
    {
        return new ConvertedFile(MimeType.Csv, fileName, content, null);
    }
    
    public static ConvertedFile FromXlsx(string fileName, byte[] stream)
    {
        return new ConvertedFile(MimeType.Xslx, fileName, null, stream);
    }
}

public enum MimeType
{
    Zip,
    Csv,
    Xslx
}