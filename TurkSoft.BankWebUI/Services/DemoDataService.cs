using System.Globalization;
using TurkSoft.BankWebUI.Models;
using TurkSoft.BankWebUI.ViewModels;

namespace TurkSoft.BankWebUI.Services
{
    public sealed class DemoDataService : IDemoDataService
    {
        private static readonly object _lock = new();
        private static bool _seeded;

        private static readonly List<DemoUser> _users = new();
        private static readonly List<BankIntegrationSetting> _bankIntegrations = new();
        private static readonly List<BankPullLog> _bankPullLogs = new();
        private static readonly List<BankTransaction> _tx = new();
        private static readonly List<AccountMapping> _mappings = new();
        private static readonly List<GlAccount> _glAccounts = new();
        private static readonly List<Vendor> _vendors = new();
        private static readonly List<TransferRecord> _transfer = new();
        private static readonly List<TransferQueueItem> _queue = new();
        private static readonly List<TransferErrorLog> _errors = new();

        private static int _txId = 1;
        private static int _transferId = 1;
        private static int _queueId = 1;
        private static int _errorId = 1;
        private static int _pullLogId = 1;

        private static readonly string[] Banks = new[]
        {
        "Ziraat", "VakıfBank", "Halkbank", "İş Bankası", "Garanti BBVA", "Akbank", "Yapı Kredi"
    };

        private static readonly string[] AccountTypes = new[]
        {
        "Vadesiz", "Vadeli", "Kredi", "POS", "Kredi Kartı", "Döviz"
    };

        public DemoDataService() => EnsureSeed();

        private static void EnsureSeed()
        {
            lock (_lock)
            {
                if (_seeded) return;

                // Demo users
                _users.AddRange(new[]
                {
                new DemoUser { Id=1, FullName="Admin Kullanıcı", Email="admin@firma.com", Password="123456", Role="Admin", IsActive=true },
                new DemoUser { Id=2, FullName="Finans Uzmanı", Email="finans@firma.com", Password="123456", Role="Finance", IsActive=true },
                new DemoUser { Id=3, FullName="Entegrasyon", Email="integrator@firma.com", Password="123456", Role="Integrator", IsActive=true },
                new DemoUser { Id=4, FullName="Sadece İzleme", Email="viewer@firma.com", Password="123456", Role="Viewer", IsActive=true },
            });

                // Master Data: GL Accounts
                _glAccounts.AddRange(new[]
                {
                new GlAccount{ Code="102.01", Name="Bankalar", IsActive=true },
                new GlAccount{ Code="102.02", Name="Vadeli Mevduat", IsActive=true },
                new GlAccount{ Code="102.50", Name="Döviz Banka Hesapları", IsActive=true },
                new GlAccount{ Code="108.01", Name="Diğer Hazır Değerler (POS)", IsActive=true },
                new GlAccount{ Code="300.01", Name="Banka Kredileri", IsActive=true },
                new GlAccount{ Code="320.10", Name="Kredi Kartı Borçları", IsActive=true },
                new GlAccount{ Code="780.01", Name="Banka Masrafları", IsActive=true },
            });

                // Master Data: Vendors (Cari)
                _vendors.AddRange(new[]
                {
                new Vendor{ Code="CARI-0001", Name="ABC Tedarik A.Ş.", IsActive=true },
                new Vendor{ Code="CARI-0002", Name="XYZ Lojistik Ltd.", IsActive=true },
                new Vendor{ Code="CARI-0003", Name="Delta Yazılım A.Ş.", IsActive=true },
                new Vendor{ Code="CARI-0004", Name="Omega Enerji A.Ş.", IsActive=true },
            });

                // Bank integration settings
                var today = DateTime.Today;
                foreach (var b in Banks)
                {
                    _bankIntegrations.Add(new BankIntegrationSetting
                    {
                        BankName = b,
                        ConnectionType = b is "Ziraat" or "Garanti BBVA" ? ConnectionType.Api : ConnectionType.HostToHost,
                        IsActive = b != "Halkbank",
                        PullSchedule = "Her gün 06:00",
                        LastPullAt = today.AddDays(-1).AddHours(6),
                        NextPullAt = today.AddHours(6).AddDays(today.Hour >= 6 ? 1 : 0),
                        LastStatus = b == "Halkbank" ? BankPullStatus.Warning : BankPullStatus.Ok,
                        LastMessage = b == "Halkbank" ? "Kimlik doğrulama yenileme gerekiyor (demo)" : "Başarılı",
                        CredentialsConfigured = b != "Halkbank"
                    });
                }

                // Default mappings (bank + hesap tipi)
                int mid = 1;
                foreach (var b in Banks)
                {
                    _mappings.Add(new AccountMapping { Id = mid++, BankName = b, AccountType = "Vadesiz", GlAccountCode = "102.01", GlAccountName = "Bankalar", CostCenter = "FIN", IsDefault = true });
                    _mappings.Add(new AccountMapping { Id = mid++, BankName = b, AccountType = "Vadeli", GlAccountCode = "102.02", GlAccountName = "Vadeli Mevduat", CostCenter = "FIN", IsDefault = true });
                    _mappings.Add(new AccountMapping { Id = mid++, BankName = b, AccountType = "POS", GlAccountCode = "108.01", GlAccountName = "Diğer Hazır Değerler (POS)", CostCenter = "SAT", IsDefault = true });
                    _mappings.Add(new AccountMapping { Id = mid++, BankName = b, AccountType = "Kredi", GlAccountCode = "300.01", GlAccountName = "Banka Kredileri", CostCenter = "FIN", IsDefault = true });
                    _mappings.Add(new AccountMapping { Id = mid++, BankName = b, AccountType = "Döviz", GlAccountCode = "102.50", GlAccountName = "Döviz Banka Hesapları", CostCenter = "FIN", IsDefault = true });
                    _mappings.Add(new AccountMapping { Id = mid++, BankName = b, AccountType = "Kredi Kartı", GlAccountCode = "320.10", GlAccountName = "Kredi Kartı Borçları", CostCenter = "FIN", IsDefault = true });
                }

                // Transactions (son 14 gün demo)
                var rnd = new Random(42);
                DateTime start = DateTime.Today.AddDays(-13);
                for (int d = 0; d < 14; d++)
                {
                    var day = start.AddDays(d);
                    int count = rnd.Next(3, 8);

                    for (int i = 0; i < count; i++)
                    {
                        var bank = Banks[rnd.Next(Banks.Length)];
                        var type = AccountTypes[rnd.Next(AccountTypes.Length)];
                        var isIn = rnd.NextDouble() > 0.45;

                        var amount = Math.Round((decimal)(rnd.Next(2000, 180000) + rnd.NextDouble()), 2);
                        var debit = isIn ? 0m : amount;
                        var credit = isIn ? amount : 0m;

                        var refNo = $"BNK-{day:yyMMdd}-{_txId:0000}";
                        var desc = type switch
                        {
                            "POS" => "POS toplu tahsilat",
                            "Kredi" => "Kredi faiz/taksit",
                            "Döviz" => "Swift / kur farkı",
                            "Kredi Kartı" => "Kart hareketi",
                            _ => (isIn ? "EFT tahsilat" : "Havale/EFT ödeme")
                        };

                        _tx.Add(new BankTransaction
                        {
                            Id = _txId++,
                            Date = day.AddMinutes(rnd.Next(30, 600)),
                            BankName = bank,
                            AccountType = type,
                            ReferenceNo = refNo,
                            Description = desc,
                            Debit = debit,
                            Credit = credit
                        });
                    }
                }

                // Transfer records (son 25 tx)
                var latest = _tx.OrderByDescending(x => x.Date).Take(25).ToList();
                foreach (var t in latest)
                {
                    var map = _mappings.FirstOrDefault(m => m.BankName == t.BankName && m.AccountType == t.AccountType);

                    // bazılarını bilerek eşleştirmesiz bırak
                    bool mapped = (t.Id % 4) != 0;

                    var vendor = (t.Id % 3 == 0) ? _vendors[(t.Id / 3) % _vendors.Count] : null;

                    _transfer.Add(new TransferRecord
                    {
                        Id = _transferId++,
                        TransactionId = t.Id,
                        BankName = t.BankName,
                        Date = t.Date.Date,
                        AccountType = t.AccountType,
                        ReferenceNo = t.ReferenceNo,
                        Description = t.Description,
                        Debit = t.Debit,
                        Credit = t.Credit,
                        GlAccountCode = mapped ? map?.GlAccountCode : null,
                        GlAccountName = mapped ? map?.GlAccountName : null,
                        CostCenter = mapped ? map?.CostCenter : null,
                        VendorCode = vendor?.Code,
                        VendorName = vendor?.Name,
                        Status = mapped ? TransferRecordStatus.Ready : TransferRecordStatus.Draft
                    });
                }

                // Queue + errors demo
                var readyOnes = _transfer.Where(t => t.Status == TransferRecordStatus.Ready).Take(5).ToList();
                foreach (var tr in readyOnes)
                {
                    tr.Status = TransferRecordStatus.Queued;
                    _queue.Add(new TransferQueueItem
                    {
                        Id = _queueId++,
                        TransferRecordId = tr.Id,
                        Status = QueueStatus.Pending,
                        AttemptCount = 0,
                        CreatedAt = DateTime.Now.AddMinutes(-rnd.Next(15, 240))
                    });
                }

                // 1 failed + error log
                if (_queue.Count > 0)
                {
                    var q = _queue.Last();
                    q.Status = QueueStatus.Failed;
                    q.AttemptCount = 2;
                    q.UpdatedAt = DateTime.Now.AddMinutes(-8);
                    q.LastError = "Muhasebe API timeout (demo)";

                    var tr = _transfer.First(x => x.Id == q.TransferRecordId);
                    _errors.Add(new TransferErrorLog
                    {
                        Id = _errorId++,
                        QueueItemId = q.Id,
                        CreatedAt = DateTime.Now.AddMinutes(-8),
                        BankName = tr.BankName,
                        ReferenceNo = tr.ReferenceNo,
                        Message = q.LastError!
                    });
                }

                // Seed a few pull logs
                var settings = _bankIntegrations.Take(3).ToList();
                foreach (var s in settings)
                {
                    _bankPullLogs.Add(new BankPullLog
                    {
                        Id = _pullLogId++,
                        SettingId = s.Id,
                        BankName = s.BankName,
                        FromAt = DateTime.Today.AddDays(-1).AddHours(6),
                        ToAt = DateTime.Today.AddDays(-1).AddHours(6).AddMinutes(1),
                        Status = BankPullStatus.Ok,
                        Message = "Başarılı (seed demo)",
                        NewTransactionCount = 2
                    });
                }

                _seeded = true;
            }
        }

        public DemoUser? ValidateUser(string email, string password)
        {
            email = email.Trim().ToLowerInvariant();
            return _users.FirstOrDefault(u =>
                u.IsActive &&
                u.Email.ToLowerInvariant() == email &&
                u.Password == password);
        }

        public DashboardVm GetDashboard()
        {
            var today = DateTime.Today;
            var todays = _tx.Where(x => x.Date.Date == today).ToList();

            var bankBalances = _tx
                .GroupBy(x => x.BankName)
                .Select(g => new BankBalance
                {
                    BankName = g.Key,
                    BalanceTry = g.Sum(x => x.Net) + (g.Key.GetHashCode() % 2500000)
                })
                .OrderByDescending(x => x.BalanceTry)
                .ToList();

            var labels = Enumerable.Range(0, 7).Select(i => today.AddDays(-6 + i)).ToList();
            var cashflow = labels.Select(d => _tx.Where(x => x.Date.Date == d).Sum(x => x.Net)).ToList();

            return new DashboardVm
            {
                TotalBalance = bankBalances.Sum(x => x.BalanceTry),
                DailyIn = todays.Sum(x => x.Credit),
                DailyOut = todays.Sum(x => x.Debit),
                ReconcileAlerts = _transfer.Count(x => x.Status == TransferRecordStatus.Draft),
                BankBalances = bankBalances,
                LatestTransactions = _tx.OrderByDescending(x => x.Date).Take(20).ToList(),
                CashflowLabels = labels.Select(d => d.ToString("dd.MM")).ToList(),
                CashflowNet = cashflow
            };
        }

        public ReportsIndexVm GetReports(ReportFilterVm filter)
        {
            filter.Banks = Banks.ToList();
            filter.AccountTypes = AccountTypes.ToList();

            DateTime? from = null, to = null;
            if (!string.IsNullOrWhiteSpace(filter.DateRange))
            {
                var parts = filter.DateRange.Split('-', StringSplitOptions.TrimEntries);
                if (parts.Length == 2)
                {
                    if (DateTime.TryParseExact(parts[0], "dd.MM.yyyy", CultureInfo.GetCultureInfo("tr-TR"), DateTimeStyles.None, out var f))
                        from = f.Date;
                    if (DateTime.TryParseExact(parts[1], "dd.MM.yyyy", CultureInfo.GetCultureInfo("tr-TR"), DateTimeStyles.None, out var t))
                        to = t.Date;
                }
            }

            var q = _tx.AsEnumerable();
            if (from.HasValue) q = q.Where(x => x.Date.Date >= from.Value);
            if (to.HasValue) q = q.Where(x => x.Date.Date <= to.Value);
            if (!string.IsNullOrWhiteSpace(filter.Bank)) q = q.Where(x => x.BankName == filter.Bank);
            if (!string.IsNullOrWhiteSpace(filter.AccountType)) q = q.Where(x => x.AccountType == filter.AccountType);

            var rows = q.OrderByDescending(x => x.Date).Take(400).ToList();

            return new ReportsIndexVm
            {
                Filter = filter,
                Rows = rows,
                NetByAccountType = rows.GroupBy(x => x.AccountType).ToDictionary(g => g.Key, g => g.Sum(x => x.Net)),
                NetByDay = rows.GroupBy(x => x.Date.Date).OrderBy(g => g.Key).ToDictionary(g => g.Key.ToString("dd.MM"), g => g.Sum(x => x.Net))
            };
        }

        public UsersIndexVm GetUsers() => new() { Users = _users.OrderBy(u => u.Id).ToList() };

        public void AddUser(CreateUserVm vm)
        {
            var nextId = _users.Max(x => x.Id) + 1;
            _users.Add(new DemoUser
            {
                Id = nextId,
                FullName = vm.FullName.Trim(),
                Email = vm.Email.Trim(),
                Password = vm.TempPassword,
                Role = vm.Role,
                IsActive = true
            });
        }

        public BankIntegrationsVm GetBankIntegrations()
            => new() { Items = _bankIntegrations.OrderBy(x => x.BankName).ToList() };

        public void ToggleBankActive(string id)
        {
            var item = _bankIntegrations.FirstOrDefault(x => x.Id == id);
            if (item is null) return;
            item.IsActive = !item.IsActive;
            item.LastStatus = BankPullStatus.Warning;
            item.LastMessage = item.IsActive ? "Aktif edildi (demo)" : "Pasif edildi (demo)";
        }

        public void PullNow(string id)
        {
            var item = _bankIntegrations.FirstOrDefault(x => x.Id == id);
            if (item is null) return;

            var fromAt = item.LastPullAt ?? DateTime.Now.AddHours(-24);
            var toAt = DateTime.Now;

            item.LastPullAt = toAt;
            item.NextPullAt = DateTime.Today.AddDays(1).AddHours(6);

            if (!item.CredentialsConfigured)
            {
                item.LastStatus = BankPullStatus.Error;
                item.LastMessage = "Kimlik bilgileri eksik (demo).";

                _bankPullLogs.Add(new BankPullLog
                {
                    Id = _pullLogId++,
                    SettingId = item.Id,
                    BankName = item.BankName,
                    FromAt = fromAt,
                    ToAt = toAt,
                    Status = BankPullStatus.Error,
                    Message = item.LastMessage!,
                    NewTransactionCount = 0
                });
                return;
            }

            // Demo yeni bir hareket ekle
            var rnd = new Random(item.BankName.GetHashCode() ^ DateTime.Now.Millisecond);
            var type = AccountTypes[rnd.Next(AccountTypes.Length)];
            var isIn = rnd.NextDouble() > 0.35;
            var amount = Math.Round((decimal)(rnd.Next(5000, 120000) + rnd.NextDouble()), 2);

            var t = new BankTransaction
            {
                Id = _txId++,
                Date = toAt,
                BankName = item.BankName,
                AccountType = type,
                ReferenceNo = $"BNK-{DateTime.Now:yyMMdd}-{_txId:0000}",
                Description = isIn ? "Banka çekimi: tahsilat (demo)" : "Banka çekimi: ödeme (demo)",
                Debit = isIn ? 0 : amount,
                Credit = isIn ? amount : 0
            };
            _tx.Add(t);

            // Transfer record üret
            var map = _mappings.FirstOrDefault(m => m.BankName == t.BankName && m.AccountType == t.AccountType);
            var mapped = map is not null;

            _transfer.Add(new TransferRecord
            {
                Id = _transferId++,
                TransactionId = t.Id,
                BankName = t.BankName,
                Date = t.Date.Date,
                AccountType = t.AccountType,
                ReferenceNo = t.ReferenceNo,
                Description = t.Description,
                Debit = t.Debit,
                Credit = t.Credit,
                GlAccountCode = mapped ? map!.GlAccountCode : null,
                GlAccountName = mapped ? map!.GlAccountName : null,
                CostCenter = mapped ? map!.CostCenter : null,
                Status = mapped ? TransferRecordStatus.Ready : TransferRecordStatus.Draft
            });

            item.LastStatus = BankPullStatus.Ok;
            item.LastMessage = "Çekim başarılı, yeni hareket eklendi (demo).";

            _bankPullLogs.Add(new BankPullLog
            {
                Id = _pullLogId++,
                SettingId = item.Id,
                BankName = item.BankName,
                FromAt = fromAt,
                ToAt = toAt,
                Status = BankPullStatus.Ok,
                Message = item.LastMessage!,
                NewTransactionCount = 1
            });
        }

        public BankPullLogsVm GetBankPullLogs(string? bank)
        {
            var q = _bankPullLogs.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(bank))
                q = q.Where(x => x.BankName == bank);

            return new BankPullLogsVm
            {
                Bank = bank,
                Banks = Banks.ToList(),
                Logs = q.OrderByDescending(x => x.ToAt).Take(300).ToList()
            };
        }

        public BankPullDeltaVm GetDeltaByLogId(int logId)
        {
            var log = _bankPullLogs.FirstOrDefault(x => x.Id == logId);
            if (log is null)
            {
                return new BankPullDeltaVm
                {
                    LogId = logId,
                    BankName = "—",
                    FromAt = DateTime.Now.AddMinutes(-1),
                    ToAt = DateTime.Now,
                    Transactions = new()
                };
            }

            var tx = _tx.Where(t =>
                t.BankName == log.BankName &&
                t.Date >= log.FromAt &&
                t.Date <= log.ToAt
            ).OrderByDescending(x => x.Date).ToList();

            return new BankPullDeltaVm
            {
                LogId = log.Id,
                BankName = log.BankName,
                FromAt = log.FromAt,
                ToAt = log.ToAt,
                Transactions = tx
            };
        }

        public AccountingVm GetAccounting(string? dateRange, string? bank, string? status)
        {
            DateTime? from = null, to = null;

            if (!string.IsNullOrWhiteSpace(dateRange))
            {
                var parts = dateRange.Split('-', StringSplitOptions.TrimEntries);
                if (parts.Length == 2 &&
                    DateTime.TryParseExact(parts[0], "dd.MM.yyyy", CultureInfo.GetCultureInfo("tr-TR"), DateTimeStyles.None, out var f) &&
                    DateTime.TryParseExact(parts[1], "dd.MM.yyyy", CultureInfo.GetCultureInfo("tr-TR"), DateTimeStyles.None, out var t))
                {
                    from = f.Date;
                    to = t.Date;
                }
            }

            var q = _transfer.AsEnumerable();

            if (from.HasValue) q = q.Where(x => x.Date >= from.Value);
            if (to.HasValue) q = q.Where(x => x.Date <= to.Value);
            if (!string.IsNullOrWhiteSpace(bank)) q = q.Where(x => x.BankName == bank);

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TransferRecordStatus>(status, out var st))
                q = q.Where(x => x.Status == st);

            return new AccountingVm
            {
                DateRange = dateRange,
                Bank = bank,
                Status = status,
                Banks = Banks.ToList(),
                TransferRecords = q.OrderByDescending(x => x.Date).Take(300).ToList(),
                Mappings = _mappings.OrderBy(m => m.BankName).ThenBy(m => m.AccountType).ToList(),
                GlAccounts = _glAccounts.Where(x => x.IsActive).OrderBy(x => x.Code).ToList(),
                Vendors = _vendors.Where(x => x.IsActive).OrderBy(x => x.Code).ToList(),
                Queue = _queue.OrderByDescending(x => x.CreatedAt).Take(200).ToList(),
                Errors = _errors.OrderByDescending(x => x.CreatedAt).Take(200).ToList()
            };
        }

        public void ApplyMapping(int transferId, string glCode, string? costCenter, string? vendorCode)
        {
            var tr = _transfer.FirstOrDefault(x => x.Id == transferId);
            if (tr is null) return;

            var gl = _glAccounts.FirstOrDefault(x => x.Code == glCode);
            tr.GlAccountCode = glCode.Trim();
            tr.GlAccountName = gl?.Name ?? tr.GlAccountName ?? "—";
            tr.CostCenter = string.IsNullOrWhiteSpace(costCenter) ? null : costCenter.Trim();

            if (!string.IsNullOrWhiteSpace(vendorCode))
            {
                var v = _vendors.FirstOrDefault(x => x.Code == vendorCode);
                tr.VendorCode = vendorCode.Trim();
                tr.VendorName = v?.Name ?? tr.VendorName;
            }

            if (tr.Status == TransferRecordStatus.Draft)
                tr.Status = TransferRecordStatus.Ready;
        }

        public void EnqueueTransfer(int transferId)
        {
            var tr = _transfer.FirstOrDefault(x => x.Id == transferId);
            if (tr is null) return;
            if (tr.Status != TransferRecordStatus.Ready) return;

            tr.Status = TransferRecordStatus.Queued;

            _queue.Add(new TransferQueueItem
            {
                Id = _queueId++,
                TransferRecordId = tr.Id,
                Status = QueueStatus.Pending,
                AttemptCount = 0,
                CreatedAt = DateTime.Now
            });
        }

        public void ExportQueueItem(int queueId)
        {
            var q = _queue.FirstOrDefault(x => x.Id == queueId);
            if (q is null) return;

            var tr = _transfer.FirstOrDefault(x => x.Id == q.TransferRecordId);
            if (tr is null) return;

            q.Status = QueueStatus.Processing;
            q.AttemptCount++;
            q.UpdatedAt = DateTime.Now;

            // demo fail deterministik
            var fail = (HashCode.Combine(tr.Id, tr.TransactionId, tr.ReferenceNo) % 7) == 0;

            if (fail)
            {
                q.Status = QueueStatus.Failed;
                q.LastError = "Muhasebe servisinde doğrulama hatası (demo)";
                q.UpdatedAt = DateTime.Now;

                _errors.Add(new TransferErrorLog
                {
                    Id = _errorId++,
                    QueueItemId = q.Id,
                    CreatedAt = DateTime.Now,
                    BankName = tr.BankName,
                    ReferenceNo = tr.ReferenceNo,
                    Message = q.LastError!
                });
                return;
            }

            q.Status = QueueStatus.Success;
            q.LastError = null;
            q.UpdatedAt = DateTime.Now;

            tr.Status = TransferRecordStatus.Exported;
        }

        public void RetryQueueItem(int queueId)
        {
            var q = _queue.FirstOrDefault(x => x.Id == queueId);
            if (q is null) return;

            q.Status = QueueStatus.Pending;
            q.LastError = null;
            q.UpdatedAt = DateTime.Now;
        }

        public void ClearError(int errorId)
        {
            var e = _errors.FirstOrDefault(x => x.Id == errorId);
            if (e is null) return;
            _errors.Remove(e);
        }
    }
}
