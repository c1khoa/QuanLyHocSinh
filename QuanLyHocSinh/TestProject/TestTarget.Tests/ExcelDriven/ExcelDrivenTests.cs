using ClosedXML.Excel;
using NUnit.Framework; // Sử dụng NUnit cho Assertions và Test attributes
using System;
using System.IO;
using System.Linq; // Để sử dụng Skip()

namespace TestDriven.ExcelDrivenTests
{
    [TestFixture] // Đánh dấu đây là một lớp chứa các bài kiểm tra
    public class ExcelDrivenTests
    {
        // Đường dẫn tới file Excel. AppContext.BaseDirectory sẽ là thư mục bin của test project.
        private static readonly string ExcelPath = Path.Combine(
            AppContext.BaseDirectory,
            "TestData",
            "TestCases_QuanLyHocSinh.xlsx"
        );

        [Test] // Đánh dấu đây là một phương thức kiểm tra
        public void RunAllExcelTestCases()
        {
            // Bước 1: Kiểm tra xem file Excel có tồn tại không
            Assert.IsTrue(File.Exists(ExcelPath), $"Lỗi: Không tìm thấy file Excel tại đường dẫn: {ExcelPath}. " +
                                                "Hãy đảm bảo 'Copy to Output Directory' được đặt thành 'Copy always' hoặc 'Copy if newer' " +
                                                "và cấu trúc thư mục 'TestData' được sao chép đúng.");

            using var workbook = new XLWorkbook(ExcelPath);
            var sheet = workbook.Worksheet("Test Cases"); // Lấy sheet "Test Cases"

            int passedCount = 0;
            int failedCount = 0;

            // Bỏ qua dòng tiêu đề (header row)
            foreach (var row in sheet.RowsUsed().Skip(1))
            {
                // Lấy dữ liệu từ các cột
                string testID = row.Cell(1).GetString();
                string function = row.Cell(2).GetString();
                string description = row.Cell(3).GetString();
                string input = row.Cell(4).GetString();
                // Giả sử cột 'Condition' vẫn ở vị trí 5 như trong Excel bạn đã cho trước đó
                // Nếu Excel của bạn không có cột Condition thì bạn có thể bỏ dòng này hoặc sửa index
                string condition = row.Cell(5).GetString();
                string expectedResultStr = row.Cell(6).GetString();

                Console.WriteLine($"\n--- Running Test: {testID} - {description} ---");
                Console.WriteLine($"Function: {function}, Input: \"{input}\", Condition: \"{condition}\", Expected: \"{expectedResultStr}\"");

                try
                {
                    string actualResult = ""; // Biến để lưu kết quả thực tế từ ứng dụng mô phỏng
                    bool testCasePassed = false;

                    // ***** LOGIC MÔ PHỎNG THEO CÁC CHỨC NĂNG TRONG FILE EXCEL CỦA BẠN *****
                    // Các khối IF/ELSE IF này chứa LOGIC MÔ PHỎNG (giả định) cho các hàm
                    // "Đổi mật khẩu", "Thêm tài khoản", "Xem điểm".
                    // Code gọi MyCalculator hay AuthenticationService đã bị loại bỏ/comment.

                    if (function.Equals("Đổi mật khẩu", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] inputs = input.Split(',');
                        string oldPass = inputs[0];
                        string newPass = inputs[1];

                        if (newPass.Length < 5)
                        {
                            actualResult = "Mật khẩu mới không hợp lệ: mật khẩu phải có ít nhất 5 ký tự.";
                        }
                        else if (oldPass != "password123") // Giả định mật khẩu cũ đúng là "password123"
                        {
                            actualResult = "Mật khẩu cũ không đúng.";
                        }
                        else
                        {
                            actualResult = "Đổi mật khẩu thành công.";
                        }
                        testCasePassed = actualResult.Contains(expectedResultStr, StringComparison.OrdinalIgnoreCase);
                    }
                    else if (function.Equals("Thêm tài khoản", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] inputs = input.Split(',');
                        string name = inputs[0];
                        int age = int.Parse(inputs[1]);
                        // string className = inputs[2]; // Không sử dụng className trong logic mô phỏng

                        if (age < 15)
                        {
                            actualResult = "Không thể thêm học sinh dưới 15 tuổi.";
                        }
                        else
                        {
                            actualResult = "Thêm tài khoản thành công.";
                        }
                        testCasePassed = actualResult.Contains(expectedResultStr, StringComparison.OrdinalIgnoreCase);
                    }
                    else if (function.Equals("Xem điểm", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] inputs = input.Split(',');
                        string studentID = inputs[0];
                        string semester = inputs[1];

                        if (semester.Equals("HK1", StringComparison.OrdinalIgnoreCase) && studentID == "HS001")
                        {
                            actualResult = "Điểm HK1 của HS001 đã được hiển thị.";
                        }
                        else
                        {
                            actualResult = "Không tìm thấy điểm.";
                        }
                        testCasePassed = actualResult.Contains(expectedResultStr, StringComparison.OrdinalIgnoreCase);
                    }
                    else // Đây là khối xử lý nếu chức năng trong Excel không khớp với bất kỳ cái nào ở trên
                    {
                        Console.WriteLine($"WARNING: Chức năng '{function}' không được hỗ trợ trong code test này cho TestID: {testID}. Test này bị bỏ qua.");
                        testCasePassed = false; // Coi như thất bại nếu không xử lý được
                        Assert.Fail($"Chức năng '{function}' không được định nghĩa trong code test. Vui lòng kiểm tra Excel hoặc code.");
                    }

                    // So sánh kết quả thực tế (từ logic mô phỏng) với kết quả mong đợi từ Excel
                    if (testCasePassed)
                    {
                        passedCount++;
                        Console.WriteLine($"Test {testID} ✅ PASSED. Actual: '{actualResult}' | Expected: '{expectedResultStr}'");
                    }
                    else
                    {
                        failedCount++;
                        Console.WriteLine($"Test {testID} ❌ FAILED. Actual: '{actualResult}' | Expected: '{expectedResultStr}'");
                        // NUnit.Framework.Assert.Fail($"Test {testID} thất bại: {description}. Actual: '{actualResult}', Expected: '{expectedResultStr}'");
                        // Bạn có thể uncomment dòng trên nếu muốn test method dừng lại ngay khi có 1 test case fail.
                        // Hiện tại nó sẽ chỉ ghi log và tiếp tục chạy các test case khác.
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Test {testID} FAILED với ngoại lệ: {ex.Message}");
                    failedCount++;
                    // Làm cho toàn bộ NUnit test method fail nếu có bất kỳ test case nào gặp ngoại lệ
                    NUnit.Framework.Assert.Fail($"Test {testID} thất bại do ngoại lệ: {ex.Message}");
                }
            }

            Console.WriteLine($"\n--- Tóm tắt kết quả Test từ Excel ---");
            Console.WriteLine($"Tổng số Test Cases: {passedCount + failedCount}");
            Console.WriteLine($"Passed: {passedCount}");
            Console.WriteLine($"Failed: {failedCount}");

            // Đảm bảo toàn bộ NUnit test method fail nếu có bất kỳ test case nào từ Excel fail
            NUnit.Framework.Assert.AreEqual(0, failedCount, $"Có {failedCount} test case bị lỗi từ Excel.");
        }
    }
}