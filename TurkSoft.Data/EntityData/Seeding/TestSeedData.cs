using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using TurkSoft.Entities.Entities;

namespace TurkSoft.Data.EntityData.Seeding
{
    public static class TestSeedData
    {
        // Sabit bir tarih kullanıyoruz. Bu tarih her zaman aynı olmalı.
        private static readonly DateTime _fixedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static int _userIdCounter = 1;
        private static int _bankIdCounter = 1;
        private static int _accountIdCounter = 1;
        private static int _roleIdCounter = 1;
        private static int _userRoleIdCounter = 1;

        public static void Apply(ModelBuilder modelBuilder)
        {
            SeedRoles(modelBuilder);
            SeedUsers(modelBuilder);
            SeedUserRoles(modelBuilder);
            SeedBanks(modelBuilder);
            SeedBankAccounts(modelBuilder);
        }

        private static void SeedRoles(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>().HasData(
                new Role
                {
                    Id = _roleIdCounter++,
                    Name = "Admin",
                    Description = "Sistem Yöneticisi",
                    IsSystemRole = true
                },
                new Role
                {
                    Id = _roleIdCounter++,
                    Name = "User",
                    Description = "Standart Kullanıcı",
                    IsSystemRole = false
                },
                new Role
                {
                    Id = _roleIdCounter++,
                    Name = "Auditor",
                    Description = "Denetçi",
                    IsSystemRole = false
                }
            );
        }

        private static void SeedUsers(ModelBuilder modelBuilder)
        {
            // Tüm kullanıcılar için aynı hash ve salt (şifre: "P@ssw0rd123")
            var passwordHash = "AQAAAAEAACcQAAAAEHRDILxLq7nLd5AL3KJQcHlL4jNQ2hTp9hK5mR8vJ1kKjHqYtZwXvB2nMjPpLk3Gzg==";
            var passwordSalt = "SALT123456789";

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = _userIdCounter++,
                    UserName = "testuser1",
                    Email = "test1@example.com",
                    FirstName = "Test",
                    LastName = "User1",
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    IsActive = true,
                    IsEmailVerified = true,
                    CreatedDate = _fixedDate,
                    LastLoginDate = _fixedDate
                },
                new User
                {
                    Id = _userIdCounter++,
                    UserName = "testuser2",
                    Email = "test2@example.com",
                    FirstName = "Test",
                    LastName = "User2",
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    IsActive = true,
                    IsEmailVerified = true,
                    CreatedDate = _fixedDate,
                    ModifiedDate = _fixedDate
                },
                new User
                {
                    Id = _userIdCounter++,
                    UserName = "testuser3",
                    Email = "test3@example.com",
                    FirstName = "Test",
                    LastName = "User3",
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    IsActive = true,
                    IsEmailVerified = false,
                    CreatedDate = _fixedDate
                }
            );
        }

        private static void SeedUserRoles(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserRole>().HasData(
                new UserRole
                {
                    Id = _userRoleIdCounter++,
                    UserId = 1, // testuser1
                    RoleId = 1, // Admin
                    AssignedDate = _fixedDate
                },
                new UserRole
                {
                    Id = _userRoleIdCounter++,
                    UserId = 2, // testuser2
                    RoleId = 2, // User
                    AssignedDate = _fixedDate
                },
                new UserRole
                {
                    Id = _userRoleIdCounter++,
                    UserId = 3, // testuser3
                    RoleId = 3, // Auditor
                    AssignedDate = _fixedDate
                }
            );
        }

        private static void SeedBanks(ModelBuilder modelBuilder)
        {
            var banks = new List<Bank>();

            // 1. Akbank
            banks.Add(new Bank
            {
                Id = _bankIdCounter++,
                ExternalBankId = 3,
                BankName = "Akbank",
                Provider = "AkbankStatementProvider",
                UsernameLabel = "5325038",
                PasswordLabel = "J1wTegck",
                RequiresLink = false,
                RequiresTLink = false,
                RequiresAccountNumber = true,
                DefaultLink = "",      // BOŞ STRING
                DefaultTLink = "",     // BOŞ STRING
                IsActive = true,
                CreatedDate = _fixedDate
            });

            // 2. Türkiye İş Bankası
            banks.Add(new Bank
            {
                Id = _bankIdCounter++,
                ExternalBankId = 2,
                BankName = "Türkiye İş Bankası",
                Provider = "IsBankStatementProvider",
                UsernameLabel = "U18450518a",
                PasswordLabel = "Nk6Ro2Pm4w",
                RequiresLink = true,
                RequiresTLink = true,
                RequiresAccountNumber = false,
                DefaultLink = "https://posmatik2.isbank.com.tr/LoginPanel.html",
                DefaultTLink = "https://posmatik2.isbank.com.tr/Authenticate.aspx",
                IsActive = true,
                CreatedDate = _fixedDate
            });

            // 3. VakıfBank
            banks.Add(new Bank
            {
                Id = _bankIdCounter++,
                ExternalBankId = 8,
                BankName = "VakıfBank",
                Provider = "VakifBankStatementProvider",
                UsernameLabel = "FUNDA YILDIRIM",
                PasswordLabel = "AJKOTTIU",
                RequiresLink = false,
                RequiresTLink = false,
                RequiresAccountNumber = true,
                DefaultLink = "",      // BOŞ STRING
                DefaultTLink = "",     // BOŞ STRING
                IsActive = true,
                CreatedDate = _fixedDate
            });

            // 4. Kuveyt Türk
            banks.Add(new Bank
            {
                Id = _bankIdCounter++,
                ExternalBankId = 7,
                BankName = "Kuveyt Türk",
                Provider = "KuveytTurkStatementProvider",
                UsernameLabel = "kt_teknikb",
                PasswordLabel = "4ek8PiJR4B",
                RequiresLink = false,
                RequiresTLink = false,
                RequiresAccountNumber = true,
                DefaultLink = "",      // BOŞ STRING
                DefaultTLink = "",     // BOŞ STRING
                IsActive = true,
                CreatedDate = _fixedDate
            });

            // 5. Albaraka Türk
            banks.Add(new Bank
            {
                Id = _bankIdCounter++,
                ExternalBankId = 5,
                BankName = "Albaraka Türk",
                Provider = "AlbarakaStatementProvider",
                UsernameLabel = "2604665",
                PasswordLabel = "n!FdLac3jZ",
                RequiresLink = false,
                RequiresTLink = false,
                RequiresAccountNumber = true,
                DefaultLink = "",      // BOŞ STRING
                DefaultTLink = "",     // BOŞ STRING
                IsActive = true,
                CreatedDate = _fixedDate
            });

            // 6. Ziraat Katılım
            banks.Add(new Bank
            {
                Id = _bankIdCounter++,
                ExternalBankId = 11,
                BankName = "Ziraat Katılım",
                Provider = "ZiraatKatilimStatementProvider",
                UsernameLabel = "prod_tknkbesc_user",
                PasswordLabel = "256+sSSwew1",
                RequiresLink = false,
                RequiresTLink = false,
                RequiresAccountNumber = true,
                DefaultLink = "https://zkapigw.ziraatkatilim.com.tr:8443",
                DefaultTLink = "",     // BOŞ STRING
                IsActive = true,
                CreatedDate = _fixedDate
            });

            // 7. Türkiye Finans
            banks.Add(new Bank
            {
                Id = _bankIdCounter++,
                ExternalBankId = 13,
                BankName = "Türkiye Finans",
                Provider = "TurkiyeFinansStatementProvider",
                UsernameLabel = "TeknikbaglantiExtractUser",
                PasswordLabel = "Şifre",
                RequiresLink = false,
                RequiresTLink = false,
                RequiresAccountNumber = true,
                DefaultLink = "https://appgateway.turkiyefinans.com.tr/TurkiyeFinans.Accounting/IBankExtractIntegrationService.svc/basic",
                DefaultTLink = "",     // BOŞ STRING
                IsActive = true,
                CreatedDate = _fixedDate
            });

            // 8. Vakıf Katılım
            banks.Add(new Bank
            {
                Id = _bankIdCounter++,
                ExternalBankId = 9,
                BankName = "Vakıf Katılım",
                Provider = "VakifKatilimStatementProvider",
                UsernameLabel = "ExtUName",
                PasswordLabel = "ExtUPassword",
                RequiresLink = false,
                RequiresTLink = false,
                RequiresAccountNumber = true,
                DefaultLink = "https://boa.vakifkatilim.com.tr/BOA.Integration.WCFService/BOA.Integration.CustomerTransaction/CustomerTransactionService.svc/Basic",
                DefaultTLink = "",     // BOŞ STRING
                IsActive = true,
                CreatedDate = _fixedDate
            });

            // 9. QNB Finansbank (Maestro)
            banks.Add(new Bank
            {
                Id = _bankIdCounter++,
                ExternalBankId = 1,
                BankName = "QNB Finansbank (Maestro)",
                Provider = "QnbMaestroStatementProvider",
                UsernameLabel = "TEKNIKBAGLANTIWS",
                PasswordLabel = "Z'i8TdSb",
                RequiresLink = false,
                RequiresTLink = false,
                RequiresAccountNumber = true,
                DefaultLink = "http://fbmaestro.qnb.com.tr:9086/MaestroCoreEkstre/services/TeknikBaglantiService.TeknikBaglantiServiceHttpSoap11Endpoint/",
                DefaultTLink = "",     // BOŞ STRING
                IsActive = true,
                CreatedDate = _fixedDate
            });

            // 10. Emlakbank
            banks.Add(new Bank
            {
                Id = _bankIdCounter,
                ExternalBankId = 12,
                BankName = "Emlakbank",
                Provider = "EmlakbankStatementProvider",
                UsernameLabel = "WS4144",
                PasswordLabel = "7HqaPNzMvehSdV",
                RequiresLink = false,
                RequiresTLink = false,
                RequiresAccountNumber = true,
                DefaultLink = "https://boa.emlakbank.com.tr/BOA.Integration.WCFService/BOA.Integration.AccountStatement/AccountStatementService.svc/Basic",
                DefaultTLink = "",     // BOŞ STRING
                IsActive = true,
                CreatedDate = _fixedDate
            });

            modelBuilder.Entity<Bank>().HasData(banks);
        }

        private static void SeedBankAccounts(ModelBuilder modelBuilder)
        {
            var accounts = new List<BankAccount>();

            // Akbank hesapları (BankId = 1)
            accounts.Add(new BankAccount { Id = _accountIdCounter++, BankId = 1, AccountNumber = "43697", Currency = "TRY", IBAN = "", SubeNo = "", MusteriNo = "", IsActive = true });
            accounts.Add(new BankAccount { Id = _accountIdCounter++, BankId = 1, AccountNumber = "61268", Currency = "TRY", IBAN = "", SubeNo = "", MusteriNo = "", IsActive = true });
            accounts.Add(new BankAccount { Id = _accountIdCounter++, BankId = 1, AccountNumber = "52839", Currency = "TRY", IBAN = "", SubeNo = "", MusteriNo = "", IsActive = true });
            accounts.Add(new BankAccount { Id = _accountIdCounter++, BankId = 1, AccountNumber = "43698", Currency = "USD", IBAN = "", SubeNo = "", MusteriNo = "", IsActive = true });
            accounts.Add(new BankAccount { Id = _accountIdCounter++, BankId = 1, AccountNumber = "67203", Currency = "USD", IBAN = "", SubeNo = "", MusteriNo = "", IsActive = true });
            accounts.Add(new BankAccount { Id = _accountIdCounter++, BankId = 1, AccountNumber = "43699", Currency = "EUR", IBAN = "", SubeNo = "", MusteriNo = "", IsActive = true });
            accounts.Add(new BankAccount { Id = _accountIdCounter++, BankId = 1, AccountNumber = "66783", Currency = "EUR", IBAN = "", SubeNo = "", MusteriNo = "", IsActive = true });

            // İş Bankası hesapları (BankId = 2)
            accounts.Add(new BankAccount { Id = _accountIdCounter++, BankId = 2, AccountNumber = "3402", Currency = "TRY", IBAN = "", SubeNo = "", MusteriNo = "", IsActive = true });
            accounts.Add(new BankAccount { Id = _accountIdCounter++, BankId = 2, AccountNumber = "2326", Currency = "USD", IBAN = "", SubeNo = "", MusteriNo = "", IsActive = true });
            accounts.Add(new BankAccount { Id = _accountIdCounter++, BankId = 2, AccountNumber = "2312", Currency = "EUR", IBAN = "", SubeNo = "", MusteriNo = "", IsActive = true });
            accounts.Add(new BankAccount { Id = _accountIdCounter++, BankId = 2, AccountNumber = "2806", Currency = "GBP", IBAN = "", SubeNo = "", MusteriNo = "", IsActive = true });
            accounts.Add(new BankAccount { Id = _accountIdCounter++, BankId = 2, AccountNumber = "9720", Currency = "TRY", IBAN = "", SubeNo = "", MusteriNo = "", IsActive = true });
            accounts.Add(new BankAccount { Id = _accountIdCounter++, BankId = 2, AccountNumber = "2187", Currency = "USD", IBAN = "", SubeNo = "", MusteriNo = "", IsActive = true });
            accounts.Add(new BankAccount { Id = _accountIdCounter++, BankId = 2, AccountNumber = "2012", Currency = "EUR", IBAN = "", SubeNo = "", MusteriNo = "", IsActive = true });

            // VakıfBank hesapları (BankId = 3)
            accounts.Add(new BankAccount { Id = _accountIdCounter++, BankId = 3, AccountNumber = "158007300107405", Currency = "TRY", IBAN = "", SubeNo = "", MusteriNo = "", IsActive = true });
            accounts.Add(new BankAccount { Id = _accountIdCounter++, BankId = 3, AccountNumber = "158007303700730", Currency = "TRY", IBAN = "", SubeNo = "", MusteriNo = "", IsActive = true });
            accounts.Add(new BankAccount { Id = _accountIdCounter++, BankId = 3, AccountNumber = "158007315885912", Currency = "TRY", IBAN = "", SubeNo = "", MusteriNo = "", IsActive = true });
            accounts.Add(new BankAccount { Id = _accountIdCounter++, BankId = 3, AccountNumber = "158048013581068", Currency = "USD", IBAN = "", SubeNo = "", MusteriNo = "", IsActive = true });
            accounts.Add(new BankAccount { Id = _accountIdCounter++, BankId = 3, AccountNumber = "158048013743131", Currency = "EUR", IBAN = "", SubeNo = "", MusteriNo = "", IsActive = true });

            // Kuveyt Türk hesapları (BankId = 4)
            accounts.Add(new BankAccount { Id = _accountIdCounter++, BankId = 4, AccountNumber = "98886207-1", Currency = "TRY", IBAN = "", SubeNo = "", MusteriNo = "", IsActive = true });
            accounts.Add(new BankAccount { Id = _accountIdCounter++, BankId = 4, AccountNumber = "98886207-101", Currency = "USD", IBAN = "", SubeNo = "", MusteriNo = "", IsActive = true });
            accounts.Add(new BankAccount { Id = _accountIdCounter++, BankId = 4, AccountNumber = "98886207-102", Currency = "EUR", IBAN = "", SubeNo = "", MusteriNo = "", IsActive = true });

            // Albaraka Türk hesapları (BankId = 5)
            accounts.Add(new BankAccount { Id = _accountIdCounter++, BankId = 5, AccountNumber = "2604665-1", Currency = "TRY", IBAN = "", SubeNo = "", MusteriNo = "", IsActive = true });
            accounts.Add(new BankAccount { Id = _accountIdCounter++, BankId = 5, AccountNumber = "2604665-11", Currency = "TRY", IBAN = "", SubeNo = "", MusteriNo = "", IsActive = true });
            accounts.Add(new BankAccount { Id = _accountIdCounter++, BankId = 5, AccountNumber = "2604665-2", Currency = "USD", IBAN = "", SubeNo = "", MusteriNo = "", IsActive = true });
            accounts.Add(new BankAccount { Id = _accountIdCounter++, BankId = 5, AccountNumber = "2604665-3", Currency = "EUR", IBAN = "", SubeNo = "", MusteriNo = "", IsActive = true });

            // Ziraat Katılım hesapları (BankId = 6)
            accounts.Add(new BankAccount
            {
                Id = _accountIdCounter++,
                BankId = 6,
                AccountNumber = "126-282355-2",
                Currency = "TRY",
                IBAN = "TR440020900000282355000002",
                SubeNo = "126",
                MusteriNo = "282355",
                IsActive = true
            });

            accounts.Add(new BankAccount
            {
                Id = _accountIdCounter,
                BankId = 6,
                AccountNumber = "126-282355-3",
                Currency = "TRY",
                IBAN = "TR170020900000282355000003",
                SubeNo = "126",
                MusteriNo = "282355",
                IsActive = true
            });

            modelBuilder.Entity<BankAccount>().HasData(accounts);
        }
    }
}