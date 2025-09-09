(function () {
  'use strict';

  /* =========================
   *  Sabitler (API / Ayarlar)
   * ========================= */
  const BANKA_EKSTRE_API = 'https://localhost:7285/api/bankaekstre'; // excel-oku/pdf-oku/txt-oku
  const LUCA_API = 'https://localhost:7032/api/luca';         // login/companies/select-company/hesap-plani/fis-gonder
  const MATCHING_API = 'https://localhost:7018/api/bankaekstre';  // eslestir

  const USE_SWEETALERT = true;
  const DEFAULT_API_KEY = '1cd8c11693648aa213509c3a12738708'; // PROD’da frontendte tutmayın!

  /* =========================
   *  Keyword Map (örnek)
   * ========================= */
  const keywordMap = {
    "maaş": "771.01", "kira": "771.01", "kredi": "771.01", "elektrik": "771.01", "su": "771.01", "internet": "771.01", "telefon": "771.01",
    "yakıt": "771.01", "yemek": "771.01", "seyahat": "771.01", "konaklama": "771.01", "reklam": "771.01", "bakım": "771.01", "onarım": "771.01",
    "danışmanlık": "771.01", "temsil": "771.01", "nakliye": "771.01", "kargo": "771.01", "posta": "771.01", "sigorta": "771.01", "amortisman": "771.01",
    "faiz": "771.01", "komisyon": "771.01", "vergi": "771.01", "stopaj": "771.01", "prim": "771.01", "personel": "771.01", "malzeme": "771.01",
    "donanım": "771.01", "yazılım": "771.01", "ekipman": "771.01", "ofis": "771.01", "abonman": "771.01", "eğitim": "771.01", "tedarik": "771.01",
    "yedek": "771.01", "bsmv": "771.01", "eft ücret": "771.01", "ücret": "771.01", "fatura": "771.01", "yazarkasa": "771.01", "eft masraf": "771.01",
    "masraf": "771.01", "para iade": "771.01", "40355": "771.01"
  };

  /* ============ STATE ============ */
  let lucaToken = null;                // login sonrası (opsiyonel)
  let lucaCompanies = [];              // şirket listesi
  let selectedCompanyCode = '';        // seçilen şirket (gerçek kod = CompanyCode.Values)

  // Hesap planı
  let accountPlanData = [];
  let accountPage = 1, accountPageSize = 10;

  // Keyword tablosu
  let keyListData = [];
  let keyPage = 1, keyPageSize = 10;

  // Ekstre ve transfer
  const ekstreCache = { hareketler: [] };
  let transferData = [];
  let transferPage = 1, transferPageSize = 10;

  // Satır içi düzenleme
  let editingRowIndex = null;
  let editingOriginalCode = '';

  // Evrak sırası
  const evrakSeqByDate = Object.create(null);

  /* ========== DOM READY ========== */
  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', init);
  else init();

  function init() {
    bindUserSelect();              // #firmaSelect → login + şirket listesini doldur
    installCompanySelectWatcher(); // #mukellefSelect → şirket seçimi ile akışı başlat

    keyListData = Object.entries(keywordMap).map(([aciklama, kod]) => ({ aciklama, kod }));
    renderKeyTablePage();

    window.addEventListener('ekstre:eslesti', (e) => {
      transferData = Array.isArray(e.detail?.fisRows) ? e.detail.fisRows : [];
      transferPage = 1;
      editingRowIndex = null;
      renderTransferTablePage();
    });

    initDropzoneUpload();

    const btnMatch = document.getElementById('btnMatch');
    if (btnMatch) btnMatch.addEventListener('click', onMatchClick);

    const btnTransfer = document.getElementById('btnTransfer');
    if (btnTransfer) btnTransfer.addEventListener('click', onTransferClick);

    bindTransferTableActions();

    updateButtonStates();
  }

  /* =========================
   *  Kullanıcı Seçimi → Login
   * ========================= */
  function bindUserSelect() {
    const selUser = document.getElementById('firmaSelect');
    const selCompany = document.getElementById('mukellefSelect');
    if (!selUser) return;

    async function onUserChange() {
      if (!selUser.value) {
        lucaToken = null; lucaCompanies = []; selectedCompanyCode = '';
        clearPlanUI();
        resetCompanySelect('Önce kullanıcı seçiniz');
        updateButtonStates();
        return;
      }

      // value: "MusteriNo-Password", text: UserName
      const parts = String(selUser.value).split('-');
      if (parts.length < 2) { updateButtonStates(); return; }
      const CustumerNo = parts[0];
      const Password = parts.slice(1).join('-');
      const UserName = (selUser.options[selUser.selectedIndex]?.text || '').trim() || 'KULLANICI';

      try {
        TSLoader?.show?.('Luca', 'Luca’ya giriş yapılıyor…');
        setBusy(true, 'Luca’ya giriş yapılıyor…');
        lucaToken = await lucaLogin({ CustumerNo, UserName, Password, ApiKey: DEFAULT_API_KEY });
        setBusy(false);

        TSLoader?.show?.('Luca', 'Şirket listesi alınıyor…');
        setBusy(true, 'Şirket listesi alınıyor…');
        lucaCompanies = await getCompanies(lucaToken); // 400/404 → fallback içerir
        setBusy(false);

        populateCompanySelect(lucaCompanies); // #mukellefSelect doldur
        selectedCompanyCode = '';
        clearPlanUI();
        notifyOk('Giriş başarılı. Lütfen şirket seçiniz.');
      } catch (err) {
        setBusy(false);
        notifyError(err?.message || 'Giriş/Şirket listesi hatası.');
        resetCompanySelect('Hata oluştu');
      } finally {
        TSLoader?.hide?.();
        updateButtonStates();
      }
    }

    selUser.addEventListener('change', onUserChange);
    try { if (typeof $ !== 'undefined' && $(selUser).data('select2')) $(selUser).on('change', onUserChange); } catch { }
    if (selCompany) resetCompanySelect('Önce kullanıcı seçiniz');
  }

  /* ===========================================================
   *  Şirket Select: native + jQuery(select2) + observer + poll
   * =========================================================== */
  function installCompanySelectWatcher() {
    const ID = 'mukellefSelect';

    async function onCompanyPicked(evt) {
      const el = document.getElementById(ID);
      if (!el) return;

      const opt = el.options[el.selectedIndex];
      const selectedName = opt?.dataset?.name || opt?.text || el.value || '';
      const code = opt?.dataset?.code || selectedName; // gerçek kod

      if (!code) {
        selectedCompanyCode = '';
        clearPlanUI();
        updateButtonStates();
        return;
      }

      try {
        TSLoader?.show?.('Luca', 'Şirket etkinleştiriliyor…');
        setBusy(true, 'Şirket etkinleştiriliyor…');
        try { await selectCompany(lucaToken, code); } catch (e) { /* endpoint yoksa sorun değil */ }
        selectedCompanyCode = code;
        setBusy(false);

        TSLoader?.show?.('Luca', 'Hesap planı çekiliyor…');
        setBusy(true, 'Hesap planı çekiliyor…');
        accountPlanData = await getAccountingPlan(lucaToken);
        setBusy(false);

        accountPage = 1;
        renderAccountTablePage();
        ensureAccountCodeDatalist();
        notifyOk(`Hesap planı yüklendi (${selectedName}).`);
      } catch (err) {
        setBusy(false);
        notifyError(err?.message || 'Şirket seçimi / Hesap planı hatası.');
      } finally {
        TSLoader?.hide?.();
        updateButtonStates();
      }
    }

    // Native change
    const el0 = document.getElementById(ID);
    if (el0 && !el0._nativeBound) { el0.addEventListener('change', onCompanyPicked); el0._nativeBound = true; }

    // jQuery delegated (select2)
    try {
      if (typeof $ !== 'undefined') {
        $(document).off('.muksel');
        $(document)
          .on('change.muksel', `#${ID}`, onCompanyPicked)
          .on('select2:select.muksel', `#${ID}`, onCompanyPicked);
      }
    } catch { }

    // DOM’da yeniden yaratılırsa tekrar bağla
    const mo = new MutationObserver(() => {
      const el = document.getElementById(ID);
      if (el && !el._nativeBound) { el.addEventListener('change', onCompanyPicked); el._nativeBound = true; }
    });
    mo.observe(document.documentElement || document.body, { childList: true, subtree: true });

    // Değer değişimini kaçırmamak için hafif poll
    let lastVal = '';
    setInterval(() => {
      const el = document.getElementById(ID);
      if (!el) return;
      const v = el.value || '';
      if (v && v !== lastVal) { lastVal = v; onCompanyPicked({ type: 'poll' }); } else { lastVal = v; }
    }, 500);
  }

  /* ===========================
   *  Şirket Select doldurma
   * =========================== */
  function populateCompanySelect(companiesRaw) {
    const sel = document.getElementById('mukellefSelect');
    if (!sel) return;

    const firstNonEmpty = (o, keys) => {
      if (!o) return '';
      for (const k of keys) {
        if (o[k] != null && String(o[k]).trim() !== '') return String(o[k]).trim();
        for (const kk in o) {
          if (Object.prototype.hasOwnProperty.call(o, kk) && kk.toLowerCase() === k.toLowerCase()) {
            const v = o[kk];
            if (v != null && String(v).trim() !== '') return String(v).trim();
          }
        }
      }
      return '';
    };

    // [{CompanyCode:{Values,Name}}] / [{Code,Name}] / [{Values,Name}] hepsini normalize et
    const normalized = (Array.isArray(companiesRaw) ? companiesRaw : []).map(item => {
      const c = item?.CompanyCode || item?.companyCode || item;
      let name = firstNonEmpty(c, ['Name', 'name', 'Unvan', 'unvan', 'CompanyName', 'companyName', 'Text', 'text']);
      let code = firstNonEmpty(c, ['Code', 'code', 'Values', 'values', 'Value', 'value', 'Kod', 'kod', 'Id', 'ID', 'id', 'VergiNo', 'vergiNo', 'Vkn', 'vkn', 'MusteriNo', 'musteriNo']);
      if (!name && code) name = code;
      if (!code && name) code = name;
      return { name, code };
    }).filter(x => x.name || x.code);

    try {
      if (window.$ && $(sel).data('select2')) {
        const $s = $(sel);
        $s.prop('disabled', false).empty();
        $s.append(new Option('Şirket seçiniz...', '', true, false));
        normalized.forEach(({ name, code }) => {
          const text = (name && code && code !== name) ? `${name} (${code})` : (name || code);
          const opt = new Option(text, name, false, false); // value = ad
          opt.dataset.code = code;                          // gerçek kod
          opt.dataset.name = name;
          $s.append(opt);
        });
        $s.val('').trigger('change.select2');
        return;
      }
    } catch { }

    const html = ['<option value="">Şirket seçiniz...</option>'].concat(
      normalized.map(({ name, code }) => {
        const text = (name && code && code !== name) ? `${name} (${code})` : (name || code);
        return `<option value="${escapeHtml(name || code)}" data-code="${escapeHtml(code || name)}" data-name="${escapeHtml(name || code)}">${escapeHtml(text)}</option>`;
      })
    ).join('');
    sel.innerHTML = html;
    sel.disabled = false;
  }

  /* ==================
   *   EŞLEŞTİR
   * ================== */
  async function onMatchClick() {
    const companySelected = !!(document.getElementById('mukellefSelect')?.value);
    if (!companySelected) return notifyError('Lütfen şirket seçiniz.');
    if (!ekstreCache.hareketler.length) return notifyError('Lütfen önce ekstre dosyasını yükleyin.');
    if (!accountPlanData.length) return notifyError('Hesap planı bulunamadı.');

    const normalizedPlan = accountPlanData.map(x => ({
      Code: x.Code || x.code || x.HesapKodu || x.hesapKodu || x.accountCode || x.AccountCode || x.Kod || x.kod || '',
      Name: x.Name || x.name || x.HesapAdi || x.hesapAdi || x.accountName || x.AccountName || x.Ad || x.ad || ''
    }));

    const wrapper = {
      Request: {
        Hareketler: ekstreCache.hareketler,
        HesapKodlari: normalizedPlan,
        KeywordMap: keywordMap,
        BankaHesapKodu: findBankCode(normalizedPlan) || ''
      }
    };

    try {
      TSLoader?.show?.('Eşleme', 'Yüklenen ekstre ile hesap planı eşleştiriliyor…');
      setBusy(true, 'Eşleştirme yapılıyor…');
      const json = await postJson(join(MATCHING_API, 'eslestir'), wrapper);
      setBusy(false);

      notifyOk('Eşleştirme tamamlandı.');
      window.dispatchEvent(new CustomEvent('ekstre:eslesti', { detail: { fisRows: json } }));
    } catch (err) {
      setBusy(false);
      notifyError('Eşleştirme sırasında hata oluştu.');
      console.error(err);
    } finally {
      TSLoader?.hide?.();
      updateButtonStates();
    }
  }

  /* ==================
   *   TRANSFER
   * ================== */
  async function onTransferClick() {
    if (!Array.isArray(transferData) || transferData.length === 0) {
      return notifyError('Gönderilecek kayıt bulunamadı.');
    }

    const rows = buildLucaFisRowsFromTransferData(transferData);
    if (!rows.length) return notifyError('Geçerli fiş satırı yok.');

    const problems = validateTransferRows(rows);
    if (problems.length) return notifyError(problems[0]);

    try {
      TSLoader?.show?.('Transfer', 'Fiş satırları Luca’ya gönderiliyor…');
      setBusy(true, 'Fiş satırları Luca’ya gönderiliyor…');
      const result = await postJson(join(LUCA_API, 'fis-gonder'), rows);
      setBusy(false);

      if (result && (result.success === true || result.Success === true)) {
        TSLoader?.show?.('Transfer', 'Fiş satırları başarıyla gönderildi.');
        notifyOk('Fiş satırları başarıyla gönderildi.');
      } else {
        TSLoader?.show?.('Transfer', 'Fiş satırları gönderildi (servis success alanı döndürmedi).');
        notifyOk('Gönderim tamamlandı.');
      }
    } catch (err) {
      setBusy(false);
      TSLoader?.show?.('Transfer', 'Fiş gönderimi sırasında hata oluştu.');
      notifyError(err?.message || 'Fiş gönderimi sırasında hata oluştu.');
      console.error(err);
    } finally {
      TSLoader?.hide?.();
    }
  }

  function buildLucaFisRowsFromTransferData(list) {
    const out = [];
    for (let i = 0; i < list.length; i++) {
      const r = list[i];
      const hesapKodu = val(r, ['HesapKodu', 'hesapKodu', 'Code', 'code', 'LucaFisRow.HesapKodu']) || '';
      if (!hesapKodu) continue;

      const aciklama = val(r, ['Aciklama', 'aciklama', 'Description', 'description', 'LucaFisRow.Aciklama']) || '';
      const tarihRaw = val(r, ['Tarih', 'tarih', 'Date', 'date', 'LucaFisRow.Tarih']);
      const borcRaw = val(r, ['Borc', 'borc', 'Debit', 'debit', 'LucaFisRow.Borc']);
      const alacakRaw = val(r, ['Alacak', 'alacak', 'Credit', 'credit', 'LucaFisRow.Alacak']);
      const evrakNoRaw = val(r, ['EvrakNo', 'evrakNo', 'DocumentNo', 'documentNo', 'DocNo', 'docNo']);

      const borc = parseTrNumber(borcRaw);
      const alacak = parseTrNumber(alacakRaw);
      const tarihIso = toIsoDateString(tarihRaw);
      const evrakNo = evrakNoRaw || makeEvrakNo(tarihIso, 'BNK');

      out.push({
        HesapKodu: String(hesapKodu),
        Tarih: tarihIso || (tarihRaw == null ? '' : String(tarihRaw)),
        Aciklama: aciklama == null ? '' : String(aciklama),
        Borc: borc,
        Alacak: alacak,
        EvrakNo: String(evrakNo)
      });
    }
    return out;
  }

  /* ======================
   *  LUCA API Helpers
   * ====================== */
  async function lucaLogin(payload) {
    const res = await fetch(join(LUCA_API, 'login'), {
      method: 'POST', headers: { 'Content-Type': 'application/json' }, mode: 'cors', body: JSON.stringify(payload)
    });
    const json = await readJsonOrThrow(res);
    return json?.token || json?.data?.token || json?.result?.token || null;
  }

  async function getCompanies(token) {
    const headers = {}; if (token) headers['Authorization'] = 'Bearer ' + token;

    // ana uç
    let res = await fetch(join(LUCA_API, 'companies'), { method: 'GET', headers, mode: 'cors' });
    // 400/404 → eski uç
    if (!res.ok && (res.status === 400 || res.status === 404)) {
      res = await fetch(join(LUCA_API, 'cari-list'), { method: 'GET', headers, mode: 'cors' });
    }
    const json = await readJsonOrThrow(res);

    if (Array.isArray(json)) return json;
    if (Array.isArray(json?.data)) return json.data;
    if (Array.isArray(json?.result)) return json.result;
    return [];
  }

  async function selectCompany(token, code) {
    if (!code) throw new Error('Şirket kodu boş.');
    const headers = { 'Content-Type': 'application/json' };
    if (token) headers['Authorization'] = 'Bearer ' + token;

    let res = await fetch(join(LUCA_API, 'select-company'), {
      method: 'POST', headers, mode: 'cors', body: JSON.stringify({ code })
    });
    if (res.status === 404) {
      res = await fetch(join(LUCA_API, 'selectCompany'), {
        method: 'POST', headers, mode: 'cors', body: JSON.stringify({ code })
      });
      if (res.status === 404) {
        res = await fetch(join(LUCA_API, 'select-company?code=' + encodeURIComponent(code)), {
          method: 'GET', headers, mode: 'cors'
        });
      }
    }
    return readJsonOrThrow(res);
  }

  async function getAccountingPlan(token) {
    const headers = {}; if (token) headers['Authorization'] = 'Bearer ' + token;
    const res = await fetch(join(LUCA_API, 'hesap-plani'), { method: 'GET', headers, mode: 'cors' });
    const json = await readJsonOrThrow(res);
    if (Array.isArray(json)) return json;
    if (Array.isArray(json?.data)) return json.data;
    if (Array.isArray(json?.result)) return json.result;
    return [];
  }

  /* =========================
   *  Tabloların Render’ları
   * ========================= */
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

    const pick = (o, keys) => keys.find(k => k in o) || null;

    const rowsHtml = slice.map(o => {
      const codeKey = pick(o, ['code', 'Code', 'hesapKodu', 'HesapKodu', 'accountCode', 'AccountCode', 'kod', 'Kod', 'KOD']) || 'Code';
      const nameKey = pick(o, ['name', 'Name', 'hesapAdi', 'HesapAdi', 'accountName', 'AccountName', 'ad', 'Ad']) || 'Name';
      const code = (o && o[codeKey]) || '';
      const name = (o && o[nameKey]) || '';
      return '<tr><td>' + escapeHtml(code) + '</td><td>' + escapeHtml(name) + '</td><td class="text-end"></td></tr>';
    }).join('');

    tbody.innerHTML = rowsHtml;
    mountPager('accountCode', totalPages, accountPage, (p) => { accountPage = p; renderAccountTablePage(); });
  }

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

    const rowsHtml = slice.map(row =>
      '<tr><td>' + escapeHtml(row.aciklama) + '</td><td>' + escapeHtml(row.kod) + '</td><td class="text-end"></td></tr>'
    ).join('');

    tbody.innerHTML = rowsHtml;
    mountPager('keyCode', totalPages, keyPage, (p) => { keyPage = p; renderKeyTablePage(); });
  }

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

    const rowsHtml = slice.map((r, idx) => {
      const globalIndex = start + idx;

      const banka = val(r, ['BankaAdi', 'bankaAdi', 'Banka', 'banka']) || '';
      const hesapKodu = val(r, ['HesapKodu', 'hesapKodu', 'Code', 'code', 'LucaFisRow.HesapKodu']) || '';
      const tarihRaw = val(r, ['Tarih', 'tarih', 'Date', 'date', 'LucaFisRow.Tarih']) || '';
      const aciklama = val(r, ['Aciklama', 'aciklama', 'Description', 'description', 'LucaFisRow.Aciklama']) || '';
      const borcText = formatNumber(parseTrNumber(val(r, ['Borc', 'borc', 'Debit', 'debit', 'LucaFisRow.Borc'])));
      const alacText = formatNumber(parseTrNumber(val(r, ['Alacak', 'alacak', 'Credit', 'credit', 'LucaFisRow.Alacak'])));
      const tarihText = formatDate(tarihRaw);

      const hesapKoduCell = (editingRowIndex === globalIndex)
        ? ('<div class="input-group input-group-sm" style="max-width:220px;">' +
          '<input type="text" class="form-control" list="account-codes" id="hk-input-' + globalIndex + '" value="' + escapeHtml(hesapKodu) + '" />' +
          '</div>')
        : '<span>' + escapeHtml(hesapKodu) + '</span>';

      const actions = (editingRowIndex === globalIndex)
        ? ('<div class="d-inline-flex align-items-center gap-1">' +
          '<button class="btn btn-icon btn-sm btn-text-success" title="Kaydet" data-action="save"  data-index="' + globalIndex + '"><i class="ti ti-check"></i></button>' +
          '<button class="btn btn-icon btn-sm btn-text-danger"  title="İptal"  data-action="cancel" data-index="' + globalIndex + '"><i class="ti ti-x"></i></button>' +
          '</div>')
        : '<button class="btn btn-icon btn-sm btn-text-secondary" title="Düzenle" data-action="edit" data-index="' + globalIndex + '"><i class="ti ti-pencil"></i></button>';

      return '<tr>' +
        '<td class="control"></td>' +
        '<td class="dt-checkboxes-cell"><input type="checkbox"/></td>' +
        '<td>' + escapeHtml(banka) + '</td>' +
        '<td>' + hesapKoduCell + '</td>' +
        '<td>' + escapeHtml(tarihText) + '</td>' +
        '<td>' + escapeHtml(aciklama) + '</td>' +
        '<td>' + escapeHtml(borcText) + '</td>' +
        '<td>' + escapeHtml(alacText) + '</td>' +
        '<td class="text-end">' + actions + '</td>' +
        '</tr>';
    }).join('');

    tbody.innerHTML = rowsHtml;
    mountPager('transfer', totalPages, transferPage, (p) => { transferPage = p; renderTransferTablePage(); });
    updateButtonStates();
  }

  /* ================ Helpers ================ */
  function val(obj, keys) {
    if (!obj) return undefined;
    for (let i = 0; i < keys.length; i++) {
      const k = keys[i];
      if (k in obj) return obj[k];
      for (const kk in obj) {
        if (!Object.prototype.hasOwnProperty.call(obj, kk)) continue;
        if (String(kk).trim().toLowerCase() === String(k).trim().toLowerCase()) return obj[kk];
      }
    }
    return undefined;
  }

  function parseTrNumber(v) {
    if (v == null || v === '') return 0;
    if (typeof v === 'number') return v;
    let s = String(v).trim();
    s = s.replace(/\s/g, '').replace(/"/g, '');
    s = s.replace(/\./g, '').replace(/,/g, '.');
    const n = Number(s);
    return isNaN(n) ? 0 : n;
  }

  function bindTransferTableActions() {
    const tbl = document.getElementById('transfer-table');
    if (!tbl) return;
    const tbody = tbl.tBodies[0] || tbl.createTBody();

    tbody.addEventListener('click', (ev) => {
      const btn = ev.target.closest('button[data-action]');
      if (!btn) return;

      const action = btn.getAttribute('data-action');
      const rowIndex = parseInt(btn.getAttribute('data-index'), 10);
      if (isNaN(rowIndex) || rowIndex < 0 || rowIndex >= transferData.length) return;

      if (action === 'edit') {
        editingRowIndex = rowIndex;
        editingOriginalCode = transferData[rowIndex].HesapKodu || '';
        renderTransferTablePage();
        setTimeout(() => {
          const inp = document.getElementById('hk-input-' + rowIndex);
          if (inp) { inp.focus(); inp.select(); }
        }, 0);
      } else if (action === 'save') {
        const inp = document.getElementById('hk-input-' + rowIndex);
        const newVal = (inp && inp.value != null) ? String(inp.value).trim() : '';
        transferData[rowIndex].HesapKodu = newVal;
        editingRowIndex = null; editingOriginalCode = '';
        renderTransferTablePage();
        notifyOk('Hesap kodu güncellendi.');
      } else if (action === 'cancel') {
        if (editingRowIndex === rowIndex) transferData[rowIndex].HesapKodu = editingOriginalCode;
        editingRowIndex = null; editingOriginalCode = '';
        renderTransferTablePage();
      }
    });
  }

  function mountPager(prefix, totalPages, currentPage, onChange) {
    const tableId = (prefix === 'accountCode') ? 'accountCode-table'
      : (prefix === 'keyCode') ? 'keyCode-table'
        : 'transfer-table';

    const tbl = document.getElementById(tableId);
    if (!tbl) return;

    let pager = document.getElementById(prefix + '-pager');
    if (!pager) {
      pager = document.createElement('nav');
      pager.id = prefix + '-pager';
      pager.className = 'mt-3';
      tbl.parentNode.insertBefore(pager, tbl.nextSibling);
    }

    if (!totalPages || totalPages <= 1) { pager.innerHTML = ''; return; }

    const createItem = (p, label, disabled, active) =>
      '<li class="page-item ' + (disabled ? 'disabled ' : '') + (active ? 'active' : '') + '">' +
      '<a class="page-link" href="#" data-page="' + p + '">' + (label || p) + '</a></li>';

    const maxButtons = 7;
    let start = Math.max(1, currentPage - Math.floor(maxButtons / 2));
    let end = Math.min(totalPages, start + maxButtons - 1);
    if (end - start + 1 < maxButtons) start = Math.max(1, end - maxButtons + 1);

    let html = '<ul class="pagination justify-content-end">';
    html += createItem(currentPage - 1, '<i class="bi bi-chevron-left"></i>', currentPage === 1, false);
    for (let p = start; p <= end; p++) html += createItem(p, String(p), false, p === currentPage);
    html += createItem(currentPage + 1, '<i class="bi bi-chevron-right"></i>', currentPage === totalPages, false);
    html += '</ul>';

    pager.innerHTML = html;

    pager.querySelectorAll('a.page-link').forEach((a) => {
      a.addEventListener('click', (ev) => {
        ev.preventDefault();
        const pg = parseInt(a.getAttribute('data-page'), 10);
        if (isNaN(pg) || pg < 1 || pg > totalPages || pg === currentPage) return;
        if (onChange) onChange(pg);
      });
    });
  }

  /* =========================
   *  Dropzone: Ekstre Yükleme
   * ========================= */
  function initDropzoneUpload() {
    const formEl = document.getElementById('dropzone-basic');
    if (!formEl || !window.Dropzone) return;

    let dz = null;
    try { dz = Dropzone.forElement(formEl); } catch { dz = null; }
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
      } catch {
        try { dz = Dropzone.forElement(formEl); } catch { dz = null; }
      }
    }
    if (!dz) return;

    if (dz._ekstreBound) return;
    dz._ekstreBound = true;

    dz.on('addedfile', async (file) => {
      if (dz.files.length > 1) dz.removeFile(dz.files[0]);

      const endpoint = pickEkstreEndpoint(file?.name || '');
      if (!endpoint) {
        notifyError('Desteklenmeyen dosya türü. Yalnızca .xlsx, .xls, .pdf, .txt');
        dz.removeAllFiles(true);
        return;
      }

      try {
        const fd = new FormData();
        fd.append('Dosya', file);
        fd.append('KlasorYolu', 'temp');

        setBusy(true, 'Ekstre işleniyor…');
        const data = await postForm(BANKA_EKSTRE_API, endpoint, fd);
        setBusy(false);

        notifyOk('Dosya okuma işlemi tamamlandı.');

        const list = Array.isArray(data) ? data
          : Array.isArray(data?.data) ? data.data
            : Array.isArray(data?.result) ? data.result : [];
        ekstreCache.hareketler = list;

        window.dispatchEvent(new CustomEvent('ekstre:okundu', { detail: { endpoint, data: list } }));
        dz.removeAllFiles(true);
      } catch (err) {
        setBusy(false);
        notifyError(err?.message || 'Beklenmeyen bir hata oluştu.');
        dz.removeAllFiles(true);
      } finally {
        updateButtonStates();
      }
    });
  }

  function pickEkstreEndpoint(filename) {
    const ext = (filename.split('.').pop() || '').toLowerCase();
    if (ext === 'xlsx' || ext === 'xls') return 'excel-oku';
    if (ext === 'pdf') return 'pdf-oku';
    if (ext === 'txt') return 'txt-oku';
    return null;
  }

  /* ============== Misc Helpers ============== */
  function findBankCode(plan) {
    const needles = ['iş bank', 'isbank', 'ziraat', 'garanti', 'akbank', 'yapı kredi', 'yapi kredi', 'enpara', 'kuveyt', 'vakif', 'vakıf'];
    for (let i = 0; i < plan.length; i++) {
      const name = (plan[i].Name || '').toLowerCase();
      if (needles.some(n => name.includes(n))) return plan[i].Code || '';
    }
    return '';
  }

  async function postForm(base, endpoint, formData) {
    const url = join(base, endpoint);
    const res = await fetch(url, { method: 'POST', body: formData });
    if (!res.ok) {
      let t = ''; try { t = await res.text(); } catch { }
      throw new Error(t || ('HTTP ' + res.status + ' - ' + res.statusText));
    }
    return res.json();
  }

  async function postJson(url, body) {
    const res = await fetch(url, {
      method: 'POST', headers: { 'Content-Type': 'application/json' }, mode: 'cors', body: JSON.stringify(body)
    });
    return readJsonOrThrow(res);
  }

  async function readJsonOrThrow(res) {
    let txt = ''; try { txt = await res.text(); } catch { }
    let json = null; try { json = txt ? JSON.parse(txt) : null; } catch { }
    if (!res.ok) {
      const msg = (json && (json.message || json.error)) || txt || ('HTTP ' + res.status + ' - ' + res.statusText);
      throw new Error(msg);
    }
    return json;
  }

  function join(base, path) {
    base = String(base || ''); path = String(path || '');
    return base.replace(/\/$/, '') + '/' + path.replace(/^\//, '');
  }

  function setBusy(on, title) {
    if (!USE_SWEETALERT || !window.Swal) return;
    if (on) Swal.fire({ title: title || 'İşleniyor', allowOutsideClick: false, didOpen: () => Swal.showLoading() });
    else Swal.close();
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
      .replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;').replace(/'/g, '&#39;')
      .replace(/\//g, '&#47;').replace(/=/g, '&#61;');
  }

  function formatDate(v) {
    try {
      const iso = toIsoDateString(v);
      if (!iso) return String(v ?? '');
      const d = new Date(iso);
      if (!d || isNaN(d)) return String(v ?? '');
      return d.toLocaleDateString('tr-TR');
    } catch { return String(v ?? ''); }
  }

  function formatNumber(v) {
    const n = Number(v);
    if (isNaN(n)) return String(v ?? '');
    try { return n.toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }); }
    catch { return n.toFixed(2); }
  }

  function toIsoDateString(v) {
    if (!v && v !== 0) return '';
    if (v instanceof Date && !isNaN(v)) return v.toISOString().slice(0, 10);
    const s = String(v).trim();

    let m = s.match(/^(\d{4})[-\/\.](\d{1,2})[-\/\.](\d{1,2})$/); // yyyy-MM-dd
    if (m) { const y = +m[1], mo = +m[2], d = +m[3]; const dt = new Date(y, mo - 1, d); if (!isNaN(dt)) return dt.toISOString().slice(0, 10); }

    m = s.match(/^(\d{1,2})[-\/\.](\d{1,2})[-\/\.](\d{4})$/); // dd.MM.yyyy
    if (m) { const d = +m[1], mo = +m[2], y = +m[3]; const dt = new Date(y, mo - 1, d); if (!isNaN(dt)) return dt.toISOString().slice(0, 10); }

    m = s.match(/^(\d{2})(\d{2})(\d{4})$/); // ddMMyyyy
    if (m) { const d = +m[1], mo = +m[2], y = +m[3]; const dt = new Date(y, mo - 1, d); if (!isNaN(dt)) return dt.toISOString().slice(0, 10); }

    m = s.match(/^(\d{4})(\d{2})(\d{2})$/); // yyyyMMdd
    if (m) { const y = +m[1], mo = +m[2], d = +m[3]; const dt = new Date(y, mo - 1, d); if (!isNaN(dt)) return dt.toISOString().slice(0, 10); }

    const dt = new Date(s);
    if (!isNaN(dt)) return dt.toISOString().slice(0, 10);
    return '';
  }

  function makeEvrakNo(tarihIso, prefix = 'BK') {
    const key = (tarihIso && /^\d{4}-\d{2}-\d{2}$/.test(tarihIso)) ? tarihIso : new Date().toISOString().slice(0, 10);
    const yyyymmdd = key.replace(/-/g, '');
    const next = (evrakSeqByDate[key] = (evrakSeqByDate[key] || 0) + 1);
    return `${prefix}${yyyymmdd}-${String(next).padStart(4, '0')}`;
  }

  function ensureAccountCodeDatalist() {
    let dl = document.getElementById('account-codes');
    if (!dl) { dl = document.createElement('datalist'); dl.id = 'account-codes'; document.body.appendChild(dl); }
    if (!Array.isArray(accountPlanData) || !accountPlanData.length) { dl.innerHTML = ''; return; }
    dl.innerHTML = accountPlanData.map((o) => {
      const code = (o.Code || o.code || o.HesapKodu || o.hesapKodu || o.Kod || o.kod || '').trim();
      const name = (o.Name || o.name || o.HesapAdi || o.hesapAdi || o.Ad || o.ad || '').trim();
      if (!code) return '';
      const label = name ? `${code} - ${name}` : code;
      return `<option value="${escapeHtml(code)}" label="${escapeHtml(label)}"></option>`;
    }).join('');
  }

  function validateTransferRows(rows) {
    const issues = [];
    for (let i = 0; i < rows.length; i++) {
      const r = rows[i];
      const hk = val(r, ['HesapKodu', 'hesapKodu', 'Code', 'code', 'LucaFisRow.HesapKodu']);
      const borc = parseTrNumber(val(r, ['Borc', 'borc', 'Debit', 'debit', 'LucaFisRow.Borc']));
      const alac = parseTrNumber(val(r, ['Alacak', 'alacak', 'Credit', 'credit', 'LucaFisRow.Alacak']));
      const tarihIso = toIsoDateString(val(r, ['Tarih', 'tarih', 'Date', 'date', 'LucaFisRow.Tarih']));

      if (!hk || !String(hk).trim()) { issues.push(`Satır ${i + 1}: Hesap Kodu zorunlu.`); continue; }
      const hasB = (borc || 0) > 0, hasA = (alac || 0) > 0;
      if (hasB === hasA) issues.push(`Satır ${i + 1}: BORÇ ve ALACAK'tan yalnızca biri pozitif olmalı.`);
      if (!tarihIso) issues.push(`Satır ${i + 1}: Tarih formatı anlaşılamadı.`);
    }
    return issues;
  }

  function updateButtonStates() {
    const btnMatch = document.getElementById('btnMatch');
    const btnTransfer = document.getElementById('btnTransfer');

    const companySelected = !!(document.getElementById('mukellefSelect')?.value);
    const hasPlan = accountPlanData.length > 0;
    const hasEkstre = ekstreCache.hareketler.length > 0;

    if (btnMatch) btnMatch.disabled = !(companySelected && hasPlan && hasEkstre);

    const rows = buildLucaFisRowsFromTransferData(transferData || []);
    const problems = validateTransferRows(rows);
    if (btnTransfer) btnTransfer.disabled = !(Array.isArray(transferData) && transferData.length > 0 && problems.length === 0);
  }

  // UI yardımcıları
  function clearPlanUI() {
    accountPlanData = [];
    accountPage = 1;
    renderAccountTablePage();
    ensureAccountCodeDatalist();
  }

  function resetCompanySelect(placeholderText) {
    const sel = document.getElementById('mukellefSelect');
    if (!sel) return;
    try {
      if (window.$ && $(sel).data('select2')) {
        const $s = $(sel);
        $s.prop('disabled', true).empty();
        $s.append(new Option(placeholderText || 'Önce kullanıcı seçiniz', '', true, true));
        $s.val('').trigger('change.select2');
        return;
      }
    } catch { }
    sel.innerHTML = `<option value="">${escapeHtml(placeholderText || 'Önce kullanıcı seçiniz')}</option>`;
    sel.disabled = true;
  }

})();
