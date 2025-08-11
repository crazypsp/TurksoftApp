// View: <select id="firmaSelect" ...>
// View: <table id="accountCode-table"> ... </table>
// View: <table id="keyCode-table"> ... </table>
// View: <table id="transfer-table"> ... </table>
// View: <form class="dropzone needsclick" id="dropzone-basic">...</form>

(function () {
  // ========= API Bazları =========
  const BANKA_EKSTRE_API = 'https://localhost:7285/api/bankaekstre'; // excel-oku/pdf-oku/txt-oku
  const LUCA_API = 'https://localhost:7032/api/luca';                 // login/hesap-plani
  const MATCHING_API = 'https://localhost:7018/api/bankaekstre';      // eslestir (Swagger değil, gerçek base!)

  const USE_SWEETALERT = true;
  const DEFAULT_API_KEY = '1cd8c11693648aa213509c3a12738708';

  // ========= keyCode-table için map (Açıklama=key, HesapKodu=value) =========
  const keywordMap = {
    "maaş": "771.01.001",
    "kira": "771.01.001",
    "kredi": "771.01.001",
    "elektrik": "771.01.001",
    "su": "771.01.001",
    "internet": "771.01.001",
    "telefon": "771.01.001",
    "yakıt": "771.01.001",
    "yemek": "771.01.001",
    "seyahat": "771.01.001",
    "konaklama": "771.01.001",
    "reklam": "771.01.001",
    "bakım": "771.01.001",
    "onarım": "771.01.001",
    "danışmanlık": "771.01.001",
    "temsil": "771.01.001",
    "nakliye": "771.01.001",
    "kargo": "771.01.001",
    "posta": "771.01.001",
    "sigorta": "771.01.001",
    "amortisman": "771.01.001",
    "faiz": "771.01.001",
    "komisyon": "771.01.001",
    "vergi": "771.01.001",
    "stopaj": "771.01.001",
    "prim": "771.01.001",
    "personel": "771.01.001",
    "malzeme": "771.01.001",
    "donanım": "771.01.001",
    "yazılım": "771.01.001",
    "ekipman": "771.01.001",
    "ofis": "771.01.001",
    "abonman": "771.01.001",
    "eğitim": "771.01.001",
    "tedarik": "771.01.001",
    "yedek": "771.01.001",
    "bsmv": "771.01.001",
    "eft ücret": "771.01.001",
    "ücret": "771.01.001",
    "fatura":"771.01.001"
  };

  // ========= State =========
  // Luca
  let accountPlanData = [];
  let accountPage = 1, accountPageSize = 10;

  // Keyword listesi
  let keyListData = [];
  let keyPage = 1, keyPageSize = 10;

  // Ekstre cache (okunan hareketler)
  const ekstreCache = { hareketler: [] };

  // Eşleştirme (transfer) listesi
  let transferData = [];
  let transferPage = 1, transferPageSize = 10;

  // Satır içi düzenleme state
  let editingRowIndex = null;           // global index (transferData içindeki gerçek indeks)
  let editingOriginalCode = '';         // iptal için orijinal HesapKodu

  // ========= DOM hazır =========
  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', init);
  else init();

  function init() {
    // Firma seçilince login → plan → tablo
    bindFirmaSelect();

    // keywordMap → tablo + paginasyon
    keyListData = Object.entries(keywordMap).map(([aciklama, kod]) => ({ aciklama, kod }));
    renderKeyTablePage();

    // Eşleştirme sonucu → transfer tablosu
    window.addEventListener('ekstre:eslesti', (e) => {
      transferData = Array.isArray(e.detail && e.detail.fisRows) ? e.detail.fisRows : [];
      transferPage = 1;
      editingRowIndex = null;
      renderTransferTablePage();
    });

    // Dropzone (dosya seç → banka ekstre API → cache)
    initDropzoneUpload();

    // Eşleştirme butonu
    const btnMatch = document.getElementById('btnMatch');
    if (btnMatch) btnMatch.addEventListener('click', onMatchClick);

    // Transfer tablosunda işlem butonları için event delegation
    bindTransferTableActions();

    updateButtonStates();
  }

  // ========= Firma Select =========
  function bindFirmaSelect() {
    const sel = document.getElementById('firmaSelect');
    if (!sel) return;

    const handler = async () => {
      if (!sel.value) {
        accountPlanData = [];
        accountPage = 1;
        renderAccountTablePage();
        updateButtonStates();
        return;
      }
      const parts = String(sel.value).split('-');
      if (parts.length < 2) { updateButtonStates(); return; }
      const CustumerNo = parts[0];
      const Password = parts.slice(1).join('-');
      const UserName = (sel.options[sel.selectedIndex] && sel.options[sel.selectedIndex].text || '').trim() || 'KULLANICI';

      try {
        setBusy(true, 'Luca giriş yapılıyor...');
        const token = await lucaLogin({ CustumerNo, UserName, Password, ApiKey: DEFAULT_API_KEY });
        setBusy(false);

        setBusy(true, 'Hesap planı çekiliyor...');
        accountPlanData = await getAccountingPlan(token);
        setBusy(false);

        accountPage = 1;
        renderAccountTablePage();
      } catch (err) {
        setBusy(false);
        notifyError(err && err.message ? err.message : 'Luca login/hesap planı hatası.');
      } finally {
        updateButtonStates();
      }
    };

    sel.addEventListener('change', handler);
    try { if (typeof $ !== 'undefined' && $(sel).data('select2')) $(sel).on('change', handler); } catch (_) { }
  }

  // ========= EŞLEŞTİR butonu =========
  async function onMatchClick() {
    const sel = document.getElementById('firmaSelect');
    if (!sel || !sel.value) return notifyError('Lütfen firma seçiniz.');
    if (!ekstreCache.hareketler.length) return notifyError('Lütfen önce ekstre dosyasını yükleyin.');
    if (!accountPlanData.length) return notifyError('Hesap planı çekilemedi. Lütfen firma seçimini kontrol edin.');

    // Hesap planını normalize et
    const normalizedPlan = accountPlanData.map(function (x) {
      return {
        Code: x.Code || x.code || x.HesapKodu || x.hesapKodu || x.accountCode || x.AccountCode || x.Kod || x.kod || '',
        Name: x.Name || x.name || x.HesapAdi || x.hesapAdi || x.accountName || x.AccountName || x.Ad || x.ad || ''
      };
    });

    // İş Bankası kodu (varsa)
    const bankaHesapKodu = findIsBankCode(normalizedPlan) || '';

    const wrapper = {
      Request: {
        Hareketler: ekstreCache.hareketler, // BankaHareket[]
        HesapKodlari: normalizedPlan,       // AccountingCode[]
        KeywordMap: keywordMap,             // Dictionary<string,string>
        BankaHesapKodu: bankaHesapKodu      // string
      }
    };

    try {
      setBusy(true, 'Eşleştirme yapılıyor...');
      const json = await postJson(join(MATCHING_API, 'eslestir'), wrapper);
      setBusy(false);

      notifyOk('Eşleştirme tamamlandı.');
      window.dispatchEvent(new CustomEvent('ekstre:eslesti', { detail: { fisRows: json } }));
      updateButtonStates();
    } catch (err) {
      setBusy(false);
      notifyError('Eşleştirme servisine ulaşılamadı veya hata oluştu.');
      console.error(err);
    }
  }

  function updateButtonStates() {
    const btnMatch = document.getElementById('btnMatch');
    const btnTransfer = document.getElementById('btnTransfer');
    const firmSelected = !!(document.getElementById('firmaSelect') && document.getElementById('firmaSelect').value);
    const hasPlan = accountPlanData.length > 0;
    const hasEkstre = ekstreCache.hareketler.length > 0;

    if (btnMatch) btnMatch.disabled = !(firmSelected && hasPlan && hasEkstre);
    if (btnTransfer) btnTransfer.disabled = !transferData.length; // eşleştirme sonrası aktif olabilir
  }

  // ========= LUCA: login & hesap planı =========
  async function lucaLogin(payload) {
    const res = await fetch(join(LUCA_API, 'login'), {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      mode: 'cors',
      body: JSON.stringify(payload)
    });
    const json = await readJsonOrThrow(res);
    return (json && (json.token || (json.data && json.data.token) || (json.result && json.result.token))) || null;
  }

  async function getAccountingPlan(token) {
    const headers = {};
    if (token) headers['Authorization'] = 'Bearer ' + token;
    const res = await fetch(join(LUCA_API, 'hesap-plani'), {
      method: 'GET',
      headers: headers,
      mode: 'cors'
    });
    const json = await readJsonOrThrow(res);
    if (Array.isArray(json)) return json;
    if (json && Array.isArray(json.data)) return json.data;
    if (json && Array.isArray(json.result)) return json.result;
    return [];
  }

  // ========= ACCOUNT TABLE (hesap planı) + pagination =========
  function renderAccountTablePage() {
    const tbl = document.getElementById('accountCode-table');
    if (!tbl) return;
    const tbody = tbl.tBodies[0] || tbl.createTBody();

    if (!accountPlanData.length) {
      tbody.innerHTML = '<tr><td colspan="3" class="text-center text-muted">Kayıt bulunamadı</td></tr>';
      mountPager('accountCode', 0, 0, null);
      return;
    }

    const total = accountPlanData.length;
    const totalPages = Math.ceil(total / accountPageSize) || 1;
    if (accountPage > totalPages) accountPage = totalPages;

    const start = (accountPage - 1) * accountPageSize;
    const end = start + accountPageSize;
    const slice = accountPlanData.slice(start, end);

    function pick(o, keys) {
      for (var i = 0; i < keys.length; i++) if (keys[i] in o) return keys[i];
      return null;
    }

    const rowsHtml = slice.map(function (o) {
      const codeKey = pick(o, ['code', 'Code', 'hesapKodu', 'HesapKodu', 'accountCode', 'AccountCode', 'kod', 'Kod', 'KOD']) || 'Code';
      const nameKey = pick(o, ['name', 'Name', 'hesapAdi', 'HesapAdi', 'accountName', 'AccountName', 'ad', 'Ad']) || 'Name';
      const code = (o && o[codeKey]) || '';
      const name = (o && o[nameKey]) || '';
      return '<tr>'
        + '<td>' + escapeHtml(code) + '</td>'
        + '<td>' + escapeHtml(name) + '</td>'
        + '<td class="text-end"></td>'
        + '</tr>';
    }).join('');

    tbody.innerHTML = rowsHtml;
    mountPager('accountCode', totalPages, accountPage, function (p) { accountPage = p; renderAccountTablePage(); });
  }

  // ========= KEY TABLE (keywordMap) + pagination =========
  function renderKeyTablePage() {
    const tbl = document.getElementById('keyCode-table');
    if (!tbl) return;
    const tbody = tbl.tBodies[0] || tbl.createTBody();

    if (!keyListData.length) {
      tbody.innerHTML = '<tr><td colspan="3" class="text-center text-muted">Veri yok</td></tr>';
      mountPager('keyCode', 0, 0, null);
      return;
    }

    const total = keyListData.length;
    const totalPages = Math.ceil(total / keyPageSize) || 1;
    if (keyPage > totalPages) keyPage = totalPages;

    const start = (keyPage - 1) * keyPageSize;
    const end = start + keyPageSize;
    const slice = keyListData.slice(start, end);

    const rowsHtml = slice.map(function (row) {
      return '<tr>'
        + '<td>' + escapeHtml(row.aciklama) + '</td>'
        + '<td>' + escapeHtml(row.kod) + '</td>'
        + '<td class="text-end"></td>'
        + '</tr>';
    }).join('');

    tbody.innerHTML = rowsHtml;
    mountPager('keyCode', totalPages, keyPage, function (p) { keyPage = p; renderKeyTablePage(); });
  }

  // ========= TRANSFER TABLE (eşleştirme sonucu) + pagination =========
  // Tables.cshtml başlıklarına göre: 
  // [respCtrl, checkbox, Banka, Hesap Kodu, Tarih, Açıklama, Borç, Alacak, İşlemler]
  function renderTransferTablePage() {
    const tbl = document.getElementById('transfer-table');
    if (!tbl) return;
    const tbody = tbl.tBodies[0] || tbl.createTBody();

    if (!Array.isArray(transferData) || transferData.length === 0) {
      tbody.innerHTML = '<tr><td colspan="9" class="text-center text-muted">Kayıt yok</td></tr>';
      mountPager('transfer', 0, 0, null);
      updateButtonStates();
      return;
    }

    const total = transferData.length;
    const totalPages = Math.ceil(total / transferPageSize) || 1;
    if (transferPage > totalPages) transferPage = totalPages;

    const start = (transferPage - 1) * transferPageSize;
    const end = start + transferPageSize;
    const slice = transferData.slice(start, end);

    const rowsHtml = slice.map(function (r, idx) {
      const globalIndex = start + idx;

      // Alanları esnekçe yakala (CSV’de LucafisRow.* olabilir; API’de PascalCase / camelCase olabilir)
      const banka = val(r, ['BankaAdi', 'bankaAdi', 'Banka', 'banka']) || '';
      const hesapKodu = val(r, ['HesapKodu', 'hesapKodu', 'Code', 'code', 'LucaFisRow.HesapKodu']) || '';
      const tarihRaw = val(r, ['Tarih', 'tarih', 'Date', 'date', 'LucaFisRow.Tarih']) || '';
      const aciklama = val(r, ['Aciklama', 'aciklama', 'Description', 'description', 'LucaFisRow.Aciklama']) || '';
      const borcRaw = val(r, ['Borc', 'borc', 'Debit', 'debit', 'LucaFisRow.Borc']);
      const alacakRaw = val(r, ['Alacak', 'alacak', 'Credit', 'credit', 'LucaFisRow.Alacak']);

      // TR sayı çözümleme ve formatlama
      const borcNum = parseTrNumber(borcRaw);
      const alacakNum = parseTrNumber(alacakRaw);
      const borcText = formatNumber(borcNum);
      const alacakText = formatNumber(alacakNum);

      // Tarih
      const tarihText = formatDate(tarihRaw);

      // Hesap Kodu hücresi: düzenleme modunda input; değilse düz metin
      let hesapKoduCell;
      if (editingRowIndex === globalIndex) {
        hesapKoduCell =
          '<div class="input-group input-group-sm" style="max-width:220px;">' +
          '<input type="text" class="form-control" id="hk-input-' + globalIndex + '" value="' + escapeHtml(hesapKodu) + '" />' +
          '</div>';
      } else {
        hesapKoduCell = '<span>' + escapeHtml(hesapKodu) + '</span>';
      }

      // İşlemler (Vuexy ikonlu)
      let actions;
      if (editingRowIndex === globalIndex) {
        actions =
          '<div class="d-inline-flex align-items-center gap-1">' +
          '<button class="btn btn-icon btn-sm btn-text-success" title="Kaydet" data-action="save" data-index="' + globalIndex + '"><i class="ti ti-check"></i></button>' +
          '<button class="btn btn-icon btn-sm btn-text-danger"  title="İptal"  data-action="cancel" data-index="' + globalIndex + '"><i class="ti ti-x"></i></button>' +
          '</div>';
      } else {
        actions =
          '<button class="btn btn-icon btn-sm btn-text-secondary" title="Düzenle" data-action="edit" data-index="' + globalIndex + '"><i class="ti ti-pencil"></i></button>';
      }

      // İlk iki kolon (responsive control ve checkbox) Tables.cshtml’deki thead ile hizalı boş hücreler
      return '' +
        '<tr>' +
        '<td class="control"></td>' +                                   // responsive control
        '<td class="dt-checkboxes-cell"><input type="checkbox"/></td>' +// checkbox sütunu
        '<td>' + escapeHtml(banka) + '</td>' +
        '<td>' + hesapKoduCell + '</td>' +
        '<td>' + escapeHtml(tarihText) + '</td>' +
        '<td>' + escapeHtml(aciklama) + '</td>' +
        '<td>' + escapeHtml(borcText) + '</td>' +
        '<td>' + escapeHtml(alacakText) + '</td>' +
        '<td class="text-end">' + actions + '</td>' +
        '</tr>';
    }).join('');

    tbody.innerHTML = rowsHtml;
    mountPager('transfer', totalPages, transferPage, function (p) { transferPage = p; renderTransferTablePage(); });
    updateButtonStates();
  }

  /* ==================== yardımcılar ==================== */
  // r nesnesinde olası anahtar varyasyonlarını sırayla dene
  function val(obj, keys) {
    if (!obj) return undefined;
    for (var i = 0; i < keys.length; i++) {
      var k = keys[i];
      if (k in obj) return obj[k];
      // Bazı JSON’larda anahtar sonunda/başında boşluk olabiliyor
      for (var kk in obj) {
        if (Object.prototype.hasOwnProperty.call(obj, kk)) {
          if (String(kk).trim().toLowerCase() === String(k).trim().toLowerCase()) {
            return obj[kk];
          }
        }
      }
    }
    return undefined;
  }

  // "12.345,67" / "12,8" / 12.8 gibi TR sayılarını number’a çevir
  function parseTrNumber(v) {
    if (v == null || v === '') return 0;
    if (typeof v === 'number') return v;
    var s = String(v).trim();
    // CSV’de ondalık virgül, binlik nokta kullanılıyor
    // Ayrıca tırnak vb. karakterleri temizleyelim
    s = s.replace(/\s/g, '').replace(/"/g, '');
    // binlik noktaları sil, ondalık virgülü noktaya çevir
    s = s.replace(/\./g, '').replace(/,/g, '.');
    var n = Number(s);
    return isNaN(n) ? 0 : n;
  }



  // İşlem butonları (edit / save / cancel) -> event delegation
  function bindTransferTableActions() {
    const tbl = document.getElementById('transfer-table');
    if (!tbl) return;
    const tbody = tbl.tBodies[0] || tbl.createTBody();

    tbody.addEventListener('click', function (ev) {
      const btn = ev.target.closest('button[data-action]');
      if (!btn) return;

      const action = btn.getAttribute('data-action');
      const idxStr = btn.getAttribute('data-index');
      if (!idxStr) return;
      const rowIndex = parseInt(idxStr, 10);
      if (isNaN(rowIndex) || rowIndex < 0 || rowIndex >= transferData.length) return;

      if (action === 'edit') {
        // sadece tek satır düzenleyelim
        editingRowIndex = rowIndex;
        editingOriginalCode = transferData[rowIndex].HesapKodu || '';
        renderTransferTablePage();
        // fokus ver
        setTimeout(function () {
          const inp = document.getElementById('hk-input-' + rowIndex);
          if (inp) { inp.focus(); inp.select(); }
        }, 0);
      }
      else if (action === 'save') {
        const inp = document.getElementById('hk-input-' + rowIndex);
        const val = (inp && inp.value != null) ? String(inp.value).trim() : '';
        transferData[rowIndex].HesapKodu = val;
        editingRowIndex = null;
        editingOriginalCode = '';
        renderTransferTablePage();
        notifyOk('Hesap kodu güncellendi.');
      }
      else if (action === 'cancel') {
        if (editingRowIndex === rowIndex) {
          // eski değeri geri koy
          transferData[rowIndex].HesapKodu = editingOriginalCode;
        }
        editingRowIndex = null;
        editingOriginalCode = '';
        renderTransferTablePage();
      }
    });
  }

  // ========= PAGER (bootstrap tarzı) =========
  function mountPager(prefix, totalPages, currentPage, onChange) {
    var tableId = prefix === 'accountCode' ? 'accountCode-table'
      : prefix === 'keyCode' ? 'keyCode-table'
        : 'transfer-table';

    var tbl = document.getElementById(tableId);
    if (!tbl) return;

    var pager = document.getElementById(prefix + '-pager');
    if (!pager) {
      pager = document.createElement('nav');
      pager.id = prefix + '-pager';
      pager.className = 'mt-3';
      tbl.parentNode.insertBefore(pager, tbl.nextSibling);
    }

    if (!totalPages || totalPages <= 1) {
      pager.innerHTML = '';
      return;
    }

    function createPageItem(p, label, disabled, active) {
      return '<li class="page-item ' + (disabled ? 'disabled ' : '') + (active ? 'active' : '') + '">'
        + '<a class="page-link" href="#" data-page="' + p + '">' + (label || p) + '</a>'
        + '</li>';
    }

    var maxButtons = 7;
    var start = Math.max(1, (currentPage - Math.floor(maxButtons / 2)));
    var end = Math.min(totalPages, start + maxButtons - 1);
    if (end - start + 1 < maxButtons) start = Math.max(1, end - maxButtons + 1);

    var html = '<ul class="pagination justify-content-end">';
    html += createPageItem(currentPage - 1, '<i class="bi bi-chevron-left"></i>', currentPage === 1, false);
    for (var p = start; p <= end; p++) {
      html += createPageItem(p, String(p), false, p === currentPage);
    }
    html += createPageItem(currentPage + 1, '<i class="bi bi-chevron-right"></i>', currentPage === totalPages, false);
    html += '</ul>';

    pager.innerHTML = html;

    var links = pager.querySelectorAll('a.page-link');
    for (var i = 0; i < links.length; i++) {
      links[i].addEventListener('click', function (ev) {
        ev.preventDefault();
        var pg = parseInt(this.getAttribute('data-page'), 10);
        if (isNaN(pg) || pg < 1 || pg > totalPages || pg === currentPage) return;
        if (onChange) onChange(pg);
      });
    }
  }

  // ========= Dropzone: dosya seçilince Banka Ekstre API (cache) =========
  function initDropzoneUpload() {
    const formEl = document.getElementById('dropzone-basic');
    if (!formEl || !window.Dropzone) return;

    var dz = null;
    try { dz = Dropzone.forElement(formEl); } catch (_) { dz = null; }
    if (!dz) {
      try {
        dz = new Dropzone(formEl, {
          url: '/noop',
          autoProcessQueue: false,
          uploadMultiple: false,
          maxFiles: 1,
          addRemoveLinks: true,
          acceptedFiles: '.xlsx,.xls,.pdf,.txt',
          dictDefaultMessage: 'Ekstre Dosyası Seçiniz'
        });
      } catch (e) {
        try { dz = Dropzone.forElement(formEl); } catch (_) { }
      }
    }
    if (!dz) return;

    if (dz._ekstreBound) return;
    dz._ekstreBound = true;

    dz.on('addedfile', async function (file) {
      if (dz.files.length > 1) dz.removeFile(dz.files[0]);

      const endpoint = pickEkstreEndpoint((file && file.name) || '');
      if (!endpoint) {
        notifyError('Desteklenmeyen dosya türü. Yalnızca .xlsx, .xls, .pdf, .txt');
        dz.removeAllFiles(true);
        return;
      }

      try {
        const fd = new FormData();
        fd.append('Dosya', file);
        fd.append('KlasorYolu', 'temp'); // Backend boş kabul etmiyor

        setBusy(true, 'Ekstre işleniyor...');
        const data = await postForm(BANKA_EKSTRE_API, endpoint, fd);
        setBusy(false);

        notifyOk('Dosya okuma işlemi tamamlandı.');

        const list = Array.isArray(data) ? data
          : (data && Array.isArray(data.data)) ? data.data
            : (data && Array.isArray(data.result)) ? data.result
              : [];
        ekstreCache.hareketler = list;

        window.dispatchEvent(new CustomEvent('ekstre:okundu', { detail: { endpoint: endpoint, data: list } }));
        dz.removeAllFiles(true);
      } catch (err) {
        setBusy(false);
        notifyError((err && err.message) ? err.message : 'Beklenmeyen bir hata oluştu.');
        dz.removeAllFiles(true);
      } finally {
        updateButtonStates();
      }
    });
  }

  function pickEkstreEndpoint(filename) {
    const parts = filename.split('.');
    const ext = (parts.length ? parts.pop() : '').toLowerCase();
    if (ext === 'xlsx' || ext === 'xls') return 'excel-oku';
    if (ext === 'pdf') return 'pdf-oku';
    if (ext === 'txt') return 'txt-oku';
    return null;
  }

  // ========= Helpers =========
  function findIsBankCode(plan) {
    for (var i = 0; i < plan.length; i++) {
      var name = (plan[i].Name || '').toLowerCase();
      if (name.indexOf('iş bank') !== -1) return plan[i].Code || '';
    }
    return '';
  }

  async function postForm(base, endpoint, formData) {
    const url = join(base, endpoint);
    const res = await fetch(url, { method: 'POST', body: formData });
    if (!res.ok) {
      let t = '';
      try { t = await res.text(); } catch (_) { }
      throw new Error(t || ('HTTP ' + res.status + ' - ' + res.statusText));
    }
    return res.json();
  }

  async function postJson(url, body) {
    const res = await fetch(url, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      mode: 'cors',
      body: JSON.stringify(body)
    });
    return readJsonOrThrow(res);
  }

  async function readJsonOrThrow(res) {
    let txt = '';
    try { txt = await res.text(); } catch (_) { }
    let json = null;
    try { json = txt ? JSON.parse(txt) : null; } catch (_) { }
    if (!res.ok) {
      const msg = (json && (json.message || json.error)) || txt || ('HTTP ' + res.status + ' - ' + res.statusText);
      throw new Error(msg);
    }
    return json;
  }

  function join(base, path) {
    base = String(base || '');
    path = String(path || '');
    return base.replace(/\/$/, '') + '/' + path.replace(/^\//, '');
  }

  function setBusy(on, title) {
    if (!USE_SWEETALERT || !window.Swal) return;
    if (on) {
      Swal.fire({ title: title || 'İşleniyor', allowOutsideClick: false, didOpen: function () { Swal.showLoading(); } });
    } else {
      Swal.close();
    }
  }

  function notifyOk(msg) {
    if (USE_SWEETALERT && window.Swal) Swal.fire('Başarılı', msg, 'success');
    else console.log('[OK]', msg);
  }

  function notifyError(msg) {
    if (USE_SWEETALERT && window.Swal) Swal.fire('Hata', msg, 'error');
    else console.error('[HATA]', msg);
  }

  function escapeHtml(s) {
    s = String(s == null ? '' : s);
    return s
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#39;')
      .replace(/\//g, '&#47;')
      .replace(/=/g, '&#61;');
  }

  function formatDate(v) {
    try {
      const d = v ? new Date(v) : null;
      if (!d || isNaN(d)) return String(v == null ? '' : v);
      return d.toLocaleDateString('tr-TR');
    } catch (_) {
      return String(v == null ? '' : v);
    }
  }

  function formatNumber(v) {
    const n = Number(v);
    if (isNaN(n)) return String(v == null ? '' : v);
    try {
      return n.toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    } catch (_) {
      return n.toFixed(2);
    }
  }
})();
