
    document.addEventListener('DOMContentLoaded', function () {
          var email = (sessionStorage.getItem('lastLoginEmail') || '').trim().toLowerCase();
    console.log(email);
    var logo = document.getElementById('userLogo');
    //Firma Logo
    var mainOne = document.getElementById('mainOne');
    var maintwo = document.getElementById('maintwo');
    //Kullanıcı Logo
    var maintree = document.getElementById('maintree');
    var mainfour = document.getElementById('mainfour');

    var creditTotalEl     = document.getElementById('creditTotal');
    var creditRemainderEl = document.getElementById('creditRemainder');
    var creditStatusEl    = document.getElementById('creditStatus'); // Harcanan

    if (!creditTotalEl || !creditRemainderEl) return;

    // Mevcut değerleri varsayılan olarak oku (HTML'deki başlangıç değerleri)
    var defaultTotal     = parseTRInt(creditTotalEl.textContent);
    var defaultRemainder = parseTRInt(creditRemainderEl.textContent);
    var defaultSpent     = creditStatusEl ? parseTRInt(creditStatusEl.textContent)
    : Math.max(0, defaultTotal - defaultRemainder);


    if (!logo) return;

    // Email → dosya adı eşlemesi
    var map = {
        'firma@gib.com': 'Nox Müsavirim Portal-06.png',
    'mm@gib.com':    'Nox Müsavirim Portal-05.png',
    'bayi@gib.com':  'Nox Müsavirim Portal-09.png',
    'fatura@gib.com':  'Nox Müsavirim Portal-09.png'
          };

    // Email → dosya adı eşlemesi
    var mapOne = {
        'firma@gib.com': 'Nox Müsavirim Portal-01.png',
    'bayi@gib.com':  'Nox Müsavirim Portal-10.png',
    'fatura@gib.com':  'Nox Müsavirim Portal-10.png'
          };



    var basePath = '/login/images/';

    // Varsayılan (eşleşme yoksa)
    var fileName = map[email] || 'Nox Müsavirim Portal-06.png';
    var fileNameOne = mapOne[email] || 'Nox Müsavirim Portal-01.png';

    // Kaynağı güncelle
    logo.src = basePath + fileName;
    logo.style.display = 'block';

    mainOne.src = basePath + fileNameOne;
    mainOne.style.display = 'block';

    maintwo.src = basePath + fileNameOne;
    maintwo.style.display = 'block';

    maintree.src = basePath + fileName;
    maintree.style.display = 'block';

    mainfour.src = basePath + fileName;
    mainfour.style.display = 'block';

    var sirketSpan = document.getElementById('sirketname');
    if (sirketSpan) {
          var nameMap = {
        'firma@gib.com'  : 'Firma',
    'mm@gib.com'     : 'MM',
    'bayi@gib.com'   : 'Bayi',
    'fatura@gib.com' : 'Faturam Türk'
          };
    var displayName = nameMap[email] || inferNameFromEmail(email);
    if (displayName) {
        sirketSpan.textContent = displayName;
            // XS ekranlarda da görünmesini istersen aç:
            // sirketSpan.classList.remove('hidden-xs');
          }
        }


    var creditMap = {
        'firma@gib.com': {total: 10000, remainder: 10000 },
    'mm@gib.com'   : {total: 15000, remainder: 12000 },
    'bayi@gib.com' : {total: 20000, remainder: 15000 },
    'fatura@gib.com' : {total: 70000, remainder: 70000 }
          // İsterseniz remainder yerine "spent" da verebilirsiniz: {total: X, spent: Y }
        };


    var row = creditMap[email];
    if (!row) return; // Eşleşme yoksa mevcut değerleri koru

    var total = isFinite(row.total) ? row.total : defaultTotal;

    var remainder;
    if ('remainder' in row && isFinite(row.remainder)) {
        remainder = row.remainder;
        } else if ('spent' in row && isFinite(row.spent)) {
        remainder = Math.max(0, total - row.spent);
        } else {
        remainder = defaultRemainder;
        }

    // DOM'u güncelle
    creditTotalEl.textContent     = formatTRInt(total);
    creditRemainderEl.textContent = formatTRInt(remainder);

    if (creditStatusEl) {
          var spent = Math.max(0, total - remainder);
    creditStatusEl.textContent = formatTRInt(spent);
        }

    // Yardımcılar: TR format parse/format
    function parseTRInt(s) {
          if (!s) return 0;
    s = String(s).replace(/\./g, '').replace(',', '.').replace(/[^\d.]/g, '');
    var n = parseFloat(s);
    return isNaN(n) ? 0 : Math.round(n);
        }
    function formatTRInt(n) {
          try { return Number(n).toLocaleString('tr-TR'); }
    catch { return String(n); }
        }



        });

    // Stable hooks to open global modals from anywhere
    $(function () {
        $('#createInvoice').on('click', function (e) {
            e.preventDefault();
            $('#faturaOlustur').modal('show');
        });
    $('#envelopeCheckModalCol').on('click', function (e) {
        e.preventDefault();
    $('#modal-envelope').modal('show');
            });
    // bootstrap tooltips
    $('[data-toggle=\"tooltip\"]').tooltip();
        });
