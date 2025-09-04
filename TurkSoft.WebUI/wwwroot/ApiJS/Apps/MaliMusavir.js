// wwwroot/ApiJS/Apps/MaliMusavir.js
// ======================================================================
// KESİN KURAL
// - Bayi login → KullaniciBayi.list({ kullaniciId }) → BayiId[] al
//   * #fltBayiId ve #frmBayiId sadece bu BayiId'lerin BayiApi.get() sonuçlarıyla dolar
//   * Tablo: MaliMusavir.BayiId bu kümede olanlar listelenir (hard filter)
// - Admin → tüm bayiler ve tüm mali müşavirler
// - getSession() Promise/obje olabilir → toleranslı
// - CREATE/UPDATE: Bayi nav bazı projelerde zorunlu olabilir → BayiApi.get ile minimal nav ekliyoruz
// ======================================================================

import { MaliMusavirApi, BayiApi, KullaniciBayiApi } from '../entities/index.js';
import { getSession } from '../Service/LoginService.js';

// ----------------------------- Yardımcılar
const $ = s => document.querySelector(s);
const $$ = s => Array.from(document.querySelectorAll(s));
const val = el => (el?.value ?? '').trim();
const nz = s => { const t = (s ?? '').trim(); return t.length ? t : null; };
const low = s => (s ?? '').toString().toLowerCase();

function normRole(s) {
  return (s || '')
    .toString()
    .trim()
    .toLowerCase()
    .replace(/ı/g, 'i').replace(/ğ/g, 'g').replace(/ü/g, 'u')
    .replace(/ş/g, 's').replace(/ö/g, 'o').replace(/ç/g, 'c')
    .replace(/\s+/g, '');
}

function showApiError(err, fallback = 'İşlem sırasında bir hata oluştu.') {
  const p = err?.payload;
  let detail = '';
  if (p) {
    if (typeof p === 'string') detail = ` (${p})`;
    else if (p.title || p.detail) detail = ` (${p.title || p.detail})`;
    else if (p.errors && typeof p.errors === 'object') {
      const flat = Object.entries(p.errors).map(([k, v]) => `${k}: ${(Array.isArray(v) ? v.join(', ') : String(v))}`).join(' | ');
      if (flat) detail = ` (${flat})`;
    }
  }
  alert(fallback + detail);
  console.error('API Error →', err);
}

// ----------------------------- Oturum
let session = {};
let userId = null;
let roleKey = '';

async function resolveSession() {
  try {
    const maybe = getSession?.() ?? {};
    return (maybe && typeof maybe.then === 'function') ? (await maybe || {}) : (maybe || {});
  } catch {
    return {};
  }
}
function applySession(s) {
  session = s || {};
  userId = String(session.userId || session.UserId || session.Id || session.id || session.KullaniciId || '');
  roleKey = normRole(session.role || session.Role || session?.context?.type || '');
}
const isAdmin = () => roleKey === 'admin';
const isBayiUsr = () => roleKey === 'bayi';

// ----------------------------- DOM
const tbody = $('#tbl-mm-body');

// Filtre alanları
const fAd = $('#fltAdSoyad');
const fEpo = $('#fltEposta');
const fBayi = $('#fltBayiId');

// Modal / form
const btnNew = $('#btnNewMM');
const modalEl = $('#mdlMM');
const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
const formEl = $('#frmMM');

// Form alanları
const fmId = $('#frmId');
const fmAd = $('#frmAdSoyad');
const fmTel = $('#frmTelefon');
const fmEpo = $('#frmEposta');
const fmUnv = $('#frmUnvan');
const fmVergi = $('#frmVergiNo');
const fmTckn = $('#frmTCKN');
const fmBayi = $('#frmBayiId');

// ----------------------------- Select doldurucu
function fillSelect(sel, items, addEmpty = true) {
  if (!sel) return;
  sel.innerHTML = '';
  if (addEmpty) {
    const o = document.createElement('option');
    o.value = ''; o.textContent = '— Seçiniz —';
    sel.appendChild(o);
  }
  (items || []).forEach(x => {
    const o = document.createElement('option');
    o.value = x.Id;
    o.textContent = x.Unvan;
    sel.appendChild(o);
  });
  if (sel.options.length === (addEmpty ? 2 : 1)) sel.selectedIndex = addEmpty ? 1 : 0;
}

// ----------------------------- Login bayi kullanıcısının BayiId[] (STRICT)

  async function getMyBayiIdsStrict(uid) {
    if (!uid) return [];
    const sid = String(uid);

    let rows = [];
    try {
      // Sunucu parametreyi yok saysa bile yine de dönen listeden client-side filtre yapacağız
      rows = await KullaniciBayiApi.list({ kullaniciId: sid });
    } catch {
      // QS desteklenmiyorsa / hata verdiyse tüm listeyi çekelim; yine client-side filtreleyeceğiz
      rows = await KullaniciBayiApi.list();
    }

    const ids = (Array.isArray(rows) ? rows : [])
      // kesin eşleşme: kullaniciId == uid
      .filter(r => String(r.KullaniciId ?? r.kullaniciId ?? '') === sid)
      // (opsiyonel) pasif/silinmiş kaydı dışarıda bırakmak istersen şunları aç:
      // .filter(r => r.IsActive !== false)
      // .filter(r => !r.DeleteDate && !r.SilindiMi)
      // BayiId al
      .map(r => String(r.BayiId ?? r.bayiId ?? ''))
      // boşları at
      .filter(Boolean);

    // tekilleştir
    return [...new Set(ids)];
}

// Belirli BayiId[] → Bayi detayları
async function getBayilerByIds(ids = []) {
  const uniq = Array.from(new Set((ids || []).map(String))).filter(Boolean);
  if (!uniq.length) return [];
  // eşzamanlı ve güvenli
  const settled = await Promise.allSettled(uniq.map(id => BayiApi.get(id)));
  return settled
    .filter(x => x.status === 'fulfilled' && x.value?.Id)
    .map(x => x.value);
}

// ----------------------------- Lookuplar (fltBayiId & frmBayiId)
async function loadLookups() {
  if (isAdmin()) {
    const allBayiler = await BayiApi.list();
    fillSelect(fBayi, allBayiler, true);
    fillSelect(fmBayi, allBayiler, true);
    return;
  }

  // BAYİ KULLANICI: sadece kendi bayileri (STRICT)
  const myIds = await getMyBayiIdsStrict(userId);
  const myBayis = await getBayilerByIds(myIds);

  // Asla BayiApi.list() ile tüm bayi doldurma!
  fillSelect(fBayi, myBayis, true);
  fillSelect(fmBayi, myBayis, true);

  if (myBayis.length === 1) {
    fBayi && (fBayi.value = myBayis[0].Id);
    fmBayi && (fmBayi.value = myBayis[0].Id);
    // İstersen kilitle:
    // fBayi.disabled = true; fmBayi.disabled = true;
  }
}

// ----------------------------- Tablo (MM.BayiId eşleşmesi ZORUNLU)
async function loadTable() {
  const list = await MaliMusavirApi.list();

  // Admin değil → sadece kendi BayiId'leri
  let enforceSet = null;
  if (!isAdmin()) {
    const ids = await getMyBayiIdsStrict(userId);
    enforceSet = new Set(ids.map(String));
  }

  const qAd = low(val(fAd));
  const qE = low(val(fEpo));
  const qBayi = val(fBayi);

  const filtered = (list || []).filter(mm => {
    const rowBayiId = String(mm.BayiId ?? mm.bayiId ?? '');
    if (enforceSet && !enforceSet.has(rowBayiId)) return false; // hard filter
    const okBayi = !qBayi || rowBayiId === String(qBayi);
    const okAd = !qAd || low(mm.AdSoyad || '').includes(qAd);
    const okE = !qE || low(mm.Eposta || '').includes(qE);
    return okBayi && okAd && okE;
  });

  tbody.innerHTML = (filtered || []).map(r => `
    <tr>
      <td>${r.AdSoyad || ''}</td>
      <td>${r.Unvan || ''}</td>
      <td>${r.VergiNo || ''}</td>
      <td>${r.TCKN || ''}</td>
      <td>${r.Telefon || ''}</td>
      <td>${r.Eposta || ''}</td>
      <td>${r.Bayi?.Unvan || ''}</td>
      <td class="text-end">
        <button class="btn btn-sm btn-primary act-edit" data-id="${r.Id}">Düzenle</button>
        <button class="btn btn-sm btn-danger  act-del"  data-id="${r.Id}">Sil</button>
      </td>
    </tr>
  `).join('');

  bindRowActions();
}

// ----------------------------- Satır aksiyonları
function bindRowActions() {
  $$('.act-edit').forEach(b => b.addEventListener('click', async e => {
    const id = e.currentTarget.getAttribute('data-id');
    const r = await MaliMusavirApi.get(id);

    fmId.value = r.Id;
    fmAd.value = r.AdSoyad || '';
    fmTel.value = r.Telefon || '';
    fmEpo.value = r.Eposta || '';
    fmUnv.value = r.Unvan || '';
    fmVergi.value = r.VergiNo || '';
    fmTckn.value = r.TCKN || '';

    // kapsam nedeniyle listede yoksa geçici ekleyelim (örn. admin modunda değişmiş olabilir)
    const rowBayiId = String(r.BayiId ?? r.bayiId ?? '');
    if (fmBayi && rowBayiId && !Array.from(fmBayi.options).some(o => String(o.value) === rowBayiId)) {
      const opt = document.createElement('option');
      opt.value = rowBayiId;
      opt.textContent = r.Bayi?.Unvan || '(Yetki dışı Bayi)';
      fmBayi.appendChild(opt);
    }
    if (fmBayi) fmBayi.value = rowBayiId;

    modal?.show();
  }));

  $$('.act-del').forEach(b => b.addEventListener('click', async e => {
    const id = e.currentTarget.getAttribute('data-id');
    if (!confirm('Silinsin mi?')) return;
    try {
      await MaliMusavirApi.remove(id);
      await loadTable();
    } catch (err) {
      showApiError(err, 'Silme işlemi başarısız.');
    }
  }));
}

// ----------------------------- Yeni kayıt
btnNew?.addEventListener('click', () => {
  formEl?.reset();
  fmId.value = '';
  modal?.show();
});

// ----------------------------- Validasyon
function validateForm() {
  const req = [
    { el: fmAd, name: 'Ad Soyad' },
    { el: fmTel, name: 'Telefon' },
    { el: fmEpo, name: 'E-posta' },
    { el: fmUnv, name: 'Unvan' },
    { el: fmVergi, name: 'Vergi No' },
    { el: fmTckn, name: 'TCKN' },
    { el: fmBayi, name: 'Bayi' }
  ];
  const missing = req.filter(x => !val(x.el));
  if (missing.length) {
    alert('Lütfen zorunlu alanları doldurun: ' + missing.map(x => x.name).join(', '));
    return false;
  }
  return true;
}

// ----------------------------- Submit (CREATE/UPDATE)
async function buildDtoForSubmit() {
  const bayiId = val(fmBayi);

  const dto = {
    Id: nz(fmId.value) || undefined,
    AdSoyad: nz(fmAd.value),
    Telefon: nz(fmTel.value),
    Eposta: nz(fmEpo.value),
    Unvan: nz(fmUnv.value),
    VergiNo: nz(fmVergi.value),
    TCKN: nz(fmTckn.value),
    BayiId: bayiId || null
  };

  // Bazı projelerde Bayi navigation zorunlu olabilir → minimal nav ekle
  //if (bayiId) {
  //  try {
  //    const b = await BayiApi.get(bayiId);
  //    dto.Bayi = {
  //      Id: b.Id,
  //      Kod: nz(b.Kod) || '-',
  //      Unvan: nz(b.Unvan) || '-',
  //      Telefon: nz(b.Telefon) || '-',
  //      Eposta: nz(b.Eposta) || '-'
  //    };
  //  } catch {
  //    // yalnız BayiId ile devam
  //  }
  //}
  return dto;
}

formEl?.addEventListener('submit', async e => {
  e.preventDefault();
  if (!validateForm()) return;

  try {
    const dto = await buildDtoForSubmit();
    if (dto.Id) await MaliMusavirApi.update(dto.Id, dto);
    else await MaliMusavirApi.create(dto);

    modal?.hide();
    await loadTable();
  } catch (err) {
    showApiError(err, 'Mali müşavir kaydedilemedi.');
  }
});

// ----------------------------- Filtre olayları
[fAd, fEpo, fBayi].forEach(el => {
  el?.addEventListener('input', loadTable);
  el?.addEventListener('change', loadTable);
});

// ----------------------------- İlk yük
(async () => {
  applySession(await resolveSession());
  try {
    await loadLookups();   // Admin: tüm bayi | Bayi: yalnız kendi bayileri (STRICT)
    await loadTable();     // Admin: tüm MM | Bayi: yalnız kendi BayiId kümesi
  } catch (err) {
    showApiError(err, 'Ekran yüklenemedi.');
  }
})();
