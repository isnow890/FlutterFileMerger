using System.Reflection;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            // 현재 실행 파일의 위치에서 상위 디렉토리 경로 가져오기
            string currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string projectPath = Directory.GetParent(currentPath).FullName;
            string outputPath = Path.Combine(currentPath,
                $"merged_source{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss")}.txt");

            Console.WriteLine($"탐색 시작 경로: {projectPath}");

            // Migrations 폴더를 제외한 모든 .cs 파일 찾기
            var files2 = Directory.GetFiles(projectPath, "*.cs", SearchOption.AllDirectories)
                .Where(f => !f.Contains("\\Migrations\\") && !f.Contains("/Migrations/"))
                ;
            var files = files2
                .Where(f => !f.Contains("\\bin\\") && !f.Contains("/bin/"))
                .ToArray();
            
            StringBuilder mergedContent = new StringBuilder();

            // 프로젝트 시작 정보 추가
            mergedContent.AppendLine("=============================================");
            mergedContent.AppendLine($"프로젝트 경로: {projectPath}");
            mergedContent.AppendLine($"파일 병합 시간: {DateTime.Now}");
            mergedContent.AppendLine($"총 파일 수: {files.Length}");
            mergedContent.AppendLine("=============================================\n");

            // 파일 구조 트리 생성
            mergedContent.AppendLine("폴더 구조:");
            AddDirectoryStructure(projectPath, mergedContent, "");
            mergedContent.AppendLine("\n=============================================\n");

            // 각 파일의 내용을 합치기
            foreach (string file in files.OrderBy(f => f))
            {
                string relativePath = Path.GetRelativePath(projectPath, file);

                mergedContent.AppendLine("=============================================");
                mergedContent.AppendLine($"// 파일 경로: {relativePath}");
                mergedContent.AppendLine($"// 전체 경로: {file}");
                mergedContent.AppendLine($"// 파일 크기: {new FileInfo(file).Length:N0} bytes");
                mergedContent.AppendLine($"// 마지막 수정: {File.GetLastWriteTime(file)}");
                mergedContent.AppendLine("=============================================");
                mergedContent.AppendLine();
                mergedContent.AppendLine(File.ReadAllText(file));
                mergedContent.AppendLine();
                mergedContent.AppendLine();
            }

            File.WriteAllText(outputPath, mergedContent.ToString());

            Console.WriteLine($"총 {files.Length}개의 파일이 성공적으로 병합되었습니다.");
            Console.WriteLine($"결과 파일: {outputPath}");
            Console.WriteLine("아무 키나 누르면 종료됩니다...");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"에러 발생: {ex.Message}");
            Console.WriteLine("아무 키나 누르면 종료됩니다...");
            Console.ReadKey();
        }
    }

    static void AddDirectoryStructure(string path, StringBuilder sb, string indent)
    {
        // 현재 디렉토리 이름 출력
        string dirName = Path.GetFileName(path);
        if (string.IsNullOrEmpty(dirName)) dirName = path;
        sb.AppendLine($"{indent}📂 {dirName}");

        // 하위 디렉토리 처리
        foreach (var dir in Directory.GetDirectories(path).OrderBy(d => d))
        {
            AddDirectoryStructure(dir, sb, indent + "  ");
        }

        // 현재 디렉토리의 .cs 파일들 출력
        foreach (var file in Directory.GetFiles(path, "*.cs").OrderBy(f => f))
        {
            sb.AppendLine($"{indent}  📄 {Path.GetFileName(file)}");
        }
    }
}