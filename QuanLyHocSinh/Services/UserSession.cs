using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using QuanLyHocSinh.Model.Entities;

namespace QuanLyHocSinh.Services
{
public class UserSession
{
    private static UserSession? _instance;

    public static UserSession Instance => _instance ??= new UserSession();

    public string? Username { get; set; }
    public string? Role { get; set; }

    private UserSession() { }
}


}
