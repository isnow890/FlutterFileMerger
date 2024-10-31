using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            string currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string rootPath = Directory.GetParent(currentPath).FullName;
            string libPath = Path.Combine(rootPath, "lib");

            if (!Directory.Exists(libPath))
            {
                throw new DirectoryNotFoundException("lib 폴더를 찾을 수 없습니다. FileMerger 폴더가 Flutter 프로젝트 루트에 있는지 확인해주세요.");
            }

            string outputPath = Path.Combine(currentPath,
                $"merged_dart_source_{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss")}.txt");

            string[] excludePatterns = new[]
            {
                ".g.dart",
                ".freezed.dart",
                ".config.dart",
                ".gr.dart",
                ".generated.dart",
                ".mocks.dart"
            };

            var files = Directory.GetFiles(libPath, "*.dart", SearchOption.AllDirectories)
                .Where(f => !excludePatterns.Any(pattern => f.EndsWith(pattern)))
                .ToArray();
            
            StringBuilder mergedContent = new StringBuilder();
            mergedContent.AppendLine("=============================================");
            mergedContent.AppendLine($"파일 병합 시간: {DateTime.Now}");
            mergedContent.AppendLine($"총 Dart 파일 수: {files.Length}");
            mergedContent.AppendLine("=============================================\n");

            mergedContent.AppendLine("lib 디렉토리 구조:");
            AddDirectoryStructure(libPath, mergedContent, "", excludePatterns);
            mergedContent.AppendLine("\n=============================================\n");

            foreach (string file in files.OrderBy(f => f))
            {
                string relativePath = Path.GetRelativePath(libPath, file);
                mergedContent.AppendLine("=============================================");
                mergedContent.AppendLine($"// {relativePath}");
                mergedContent.AppendLine("=============================================");
                mergedContent.AppendLine();
                
                // 파일 내용 읽기 및 주석 제거
                string content = File.ReadAllText(file);
                string noComments = RemoveComments(content);
                // 빈 줄이 3개 이상 연속되는 경우 2개로 줄이기
                noComments = Regex.Replace(noComments, @"\n{3,}", "\n\n");
                // 앞뒤 공백 제거
                noComments = noComments.Trim();
                
                mergedContent.AppendLine(noComments);
                mergedContent.AppendLine();
                mergedContent.AppendLine();
            }

            File.WriteAllText(outputPath, mergedContent.ToString());

            Console.WriteLine($"총 {files.Length}개의 Dart 파일이 성공적으로 병합되었습니다.");
            Console.WriteLine($"결과 파일: {outputPath}");
            Console.WriteLine("아무 키나 누르면 종료됩니다...");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"에러 발생: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Console.WriteLine("아무 키나 누르면 종료됩니다...");
            Console.ReadKey();
        }
    }

    static string RemoveComments(string code)
    {
        // 여러 줄 주석 제거 (/* ... */)
        code = Regex.Replace(code, @"/\*[\s\S]*?\*/", "", RegexOptions.Multiline);
        
        // 문서화 주석 제거 (/// ...)
        code = Regex.Replace(code, @"///.*$", "", RegexOptions.Multiline);
        
        // 한 줄 주석 제거 (// ...)
        code = Regex.Replace(code, @"//.*$", "", RegexOptions.Multiline);
        
        return code;
    }

    static void AddDirectoryStructure(string path, StringBuilder sb, string indent, string[] excludePatterns)
    {
        string dirName = Path.GetFileName(path);
        if (string.IsNullOrEmpty(dirName)) dirName = path;
        
        if (dirName != "lib")
        {
            sb.AppendLine($"{indent}📂 {dirName}");
            indent += "  ";
        }

        foreach (var dir in Directory.GetDirectories(path).OrderBy(d => d))
        {
            AddDirectoryStructure(dir, sb, indent, excludePatterns);
        }

        foreach (var file in Directory.GetFiles(path, "*.dart")
            .Where(f => !excludePatterns.Any(pattern => f.EndsWith(pattern)))
            .OrderBy(f => f))
        {
            sb.AppendLine($"{indent}📄 {Path.GetFileName(file)}");
        }
    }
}