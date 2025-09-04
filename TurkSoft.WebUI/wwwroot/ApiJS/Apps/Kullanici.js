// Apps/Kullanici.js
// =====================================================================
// getSession() Promise olabilir → asenkron okunur.
// 1) Admin → tüm kullanıcıları görür, Cards buna göre dolar
// 2) Bayi  → OlusturanKullaniciId == session.userId olanları görür, Cards buna göre dolar
// 3) Bayi login → modal default rol "Mali Müşavir" + grpUserMM görünür
// 4) #fRol seçenekleri: Admin→hepsi | Bayi→Admin hariç | MM→Admin+Bayi hariç
// 5) Bayi login → ddlUserMM sadece kendi BayiId’sine bağlı MM’ler
// =====================================================================

import {
  KullaniciApi,
  BayiApi,
  MaliMusavirApi,
  KullaniciBayiApi,
  KullaniciMaliMusavirApi
} from '../entities/index.js';

import { getSession } from '../Service/LoginService.js';

// ---------- yardımcılar ----------
const $ = s => document.querySelector(s);
const val = el => (el?.value ?? '').trim();
const low = s => (s ?? '').toString().toLowerCase();
function pick(...sels) { for (const s of sels) { const el = document.querySelector(s); if (el) return el; } return null; }
function toast(msg, type = 'info') { console.log(`[${type}] ${msg}`); }
function roleKey(s) {
  return (s || '').toString().trim().toLowerCase()
    .replace(/ı/g, 'i').replace(/ğ/g, 'g').replace(/ü/g, 'u')
    .replace(/ş/g, 's').replace(/ö/g, 'o').replace(/ç/g, 'c')
    .replace(/\s+/g, '');
}
function selectOneOf(sel, ...cands) {
  if (!sel) return;
  const wants = cands.map(x => (x || '').toString().trim().toLowerCase());
  for (let i = 0; i < sel.options.length; i++) {
    const v = sel.options[i].value?.toLowerCase?.() ?? '';
    if (wants.includes(v)) { sel.selectedIndex = i; return; }
  }
  for (let i = 0; i < sel.options.length; i++) {
    const t = sel.options[i].text?.toLowerCase?.() ?? '';
    if (wants.includes(t)) { sel.selectedIndex = i; return; }
  }
}

// ---------- OTURUM DURUMU (async) ----------
let session = {};
let sessionUserId = null;
let sessionRoleKey = '';
let isAdmin = false;
let isBayiUser = false;
let isMmUser = false;

async function resolveSession() {
  try {
    const maybe = typeof getSession === 'function' ? getSession() : {};
    const s = (maybe && typeof maybe.then === 'function') ? await maybe : (maybe || {});
    return s || {};
  } catch (e) {
    console.warn('getSession error:', e);
    return {};
  }
}
function applySession(s) {
  session = s || {};
  const rid = session.userId || session.UserId || session.Id || session.id || session.KullaniciId || null;
  sessionUserId = rid ? String(rid) : null;

  sessionRoleKey = roleKey(session.role || session.Role || session?.context?.type || '');
  isAdmin = sessionRoleKey === 'admin';
  isBayiUser = sessionRoleKey === 'bayi';
  isMmUser = sessionRoleKey === 'malimusavir';
}

// ---------- liste / tablo / kartlar ----------
let rawUsers = [];
let all = [];
let dt = null;
const tbody = $('#userTable tbody');

function renderTable(items) {
  if (!tbody) return;
  tbody.innerHTML = (items || []).map(u => `
    <tr data-id="${u.Id}">
      <td>${u.AdSoyad ?? ''}</td>
      <td>${u.Eposta ?? ''}</td>
      <td>${u.Telefon ?? ''}</td>
      <td>${u.Rol ?? ''}</td>
      <td>${u.IsActive ? '<span class="badge bg-label-success">Aktif</span>' : '<span class="badge bg-label-secondary">Pasif</span>'}</td>
      <td class="text-end">
        <button class="btn btn-sm btn-outline-primary me-2 btn-edit"><i class="ti ti-pencil"></i></button>
        <button class="btn btn-sm btn-outline-danger btn-del"><i class="ti ti-trash"></i></button>
      </td>
    </tr>
  `).join('');
  if (window.DataTable) {
    if (dt) dt.destroy();
    dt = new window.DataTable('#userTable', { responsive: true, searching: false, lengthChange: false });
  }
}
function renderStats(items) {
  const t = $('#statTotal'), a = $('#statAdmins'), o = $('#statOther');
  t && (t.textContent = items.length);
  const adminCount = items.filter(x => roleKey(x.Rol) === 'admin').length;
  a && (a.textContent = adminCount);
  o && (o.textContent = (items.length - adminCount));
}

// Yetki filtresi: Admin → tümü, Bayi → OlusturanKullaniciId == sessionUserId
async function filterUsersBySession(users) {
  const list = Array.isArray(users) ? users : [];
  if (isAdmin) return list;
  if (isBayiUser && sessionUserId) {
    const sid = String(sessionUserId);
    return list.filter(u => String(u.OlusturanKullaniciId || '') === sid);
  }
  return [];
}

async function load() {
  const list = await KullaniciApi.list();
  rawUsers = Array.isArray(list) ? list : [];
  all = await filterUsersBySession(rawUsers);
  renderTable(all);
  renderStats(all);
}

function applyFilter() {
  const q = low($('#txtSearch')?.value ?? '').trim();
  const filtered = !q ? all : all.filter(u => {
    const hay = `${u.AdSoyad ?? ''} ${u.Eposta ?? ''} ${u.Rol ?? ''}`.toLowerCase();
    return hay.includes(q);
  });
  renderTable(filtered);
}

// ---------- modal ----------
const modalEl = $('#userModal');
const modal = modalEl ? new bootstrap.Modal(modalEl) : null;

const fmId = $('#userId');
const fmAd = $('#fAdSoyad');
const fmMail = $('#fEposta');
const fmPwd = $('#fSifre');
const fmTel = $('#fTelefon');
const fmRol = $('#fRol');
const fmProf = $('#fProfil'); // olabilir/kaldırılmış olabilir
const formErr = $('#formError');

const grpBayi = pick('#grpUserBayi', '.grp-user-bayi', '[data-group="user-bayi"]');
const grpMM = pick('#grpUserMM', '.grp-user-mm', '[data-group="user-mm"]');

const ddlBayi = pick('#ddlUserBayi', '#userBayiId', '#fBayiId', '[data-role="user-bayi"]');
const ddlMM = pick('#ddlUserMM', '#userMmId', '#fMaliMusavirId', '[data-role="user-mm"]');

function fillSelect(sel, items, getVal, getText, addEmpty = true) {
  if (!sel) return;
  sel.innerHTML = '';
  if (addEmpty) { const o = document.createElement('option'); o.value = ''; o.textContent = '— Seçiniz —'; sel.appendChild(o); }
  (items || []).forEach(x => {
    const o = document.createElement('option');
    o.value = getVal ? getVal(x) : x.Id;
    o.textContent = getText ? getText(x) : (x.Unvan || x.AdSoyad || '');
    sel.appendChild(o);
  });
}

// --- ROL SEÇENEK FİLTRESİ (#fRol) ---
// Admin → hepsi; Bayi → Admin hariç; MM → Admin ve Bayi hariç
function setRoleOptionsForLoggedIn() {
  if (!fmRol) return;
  const keepValue = fmRol.value; // mevcut seçimi korumayı dene (edit senaryosu)
  const roles = [
    { value: 'Admin', text: 'Admin' },
    { value: 'Bayi', text: 'Bayi' },
    { value: 'MaliMusavir', text: 'Mali Müşavir' },
    { value: 'Firma', text: 'Firma' }
  ];
  let allowed = roles;
  if (isAdmin) {
    allowed = roles;
  } else if (isBayiUser) {
    allowed = roles.filter(r => roleKey(r.value) !== 'admin');
  } else if (isMmUser) {
    allowed = roles.filter(r => !['admin', 'bayi'].includes(roleKey(r.value)));
  } else {
    // temkinli varsayılan
    allowed = roles.filter(r => roleKey(r.value) !== 'admin');
  }

  fmRol.innerHTML = '';
  for (const r of allowed) {
    const o = document.createElement('option');
    o.value = r.value;
    o.textContent = r.text;
    fmRol.appendChild(o);
  }

  // eski değeri koru (mümkünse)
  if (keepValue) selectOneOf(fmRol, keepValue, roleKey(keepValue));
}

// --- Bayi login → kendi BayiId’sine bağlı MM listesi ---
// KullaniciBayiApi.listByKullaniciId ? yoksa list() + filtre
async function getLoggedInBayiIds() {
  if (!sessionUserId) return [];
  try {
    if (KullaniciBayiApi && typeof KullaniciBayiApi.listByKullaniciId === 'function') {
      const rows = await KullaniciBayiApi.listByKullaniciId(sessionUserId);
      return (rows || []).map(r => String(r.BayiId));
    }
    if (KullaniciBayiApi && typeof KullaniciBayiApi.list === 'function') {
      const rows = await KullaniciBayiApi.list();
      return (rows || [])
        .filter(r => String(r.KullaniciId) === String(sessionUserId))
        .map(r => String(r.BayiId));
    }
  } catch (e) {
    console.warn('getLoggedInBayiIds error', e);
  }
  return [];
}

// rKey === 'bayi' → Admin’e Bayi seçtir
// rKey === 'malimusavir' → MM seçtir (Bayi login ise kendi BayiId’sine bağlı MM’ler)
async function loadLookupsForRole(rKey) {
  grpBayi && grpBayi.classList.add('d-none');
  grpMM && grpMM.classList.add('d-none');

  if (rKey === 'bayi') {
    if (!isAdmin) { fillSelect(ddlBayi, [], null, null, true); return; }
    const bayiler = await BayiApi.list();
    fillSelect(ddlBayi, bayiler, x => x.Id, x => x.Unvan, true);
    grpBayi && grpBayi.classList.remove('d-none');
    if (ddlBayi && ddlBayi.options.length === 2) ddlBayi.selectedIndex = 1;
  }
  else if (rKey === 'malimusavir') {
    let mmler = await MaliMusavirApi.list();
    if (isBayiUser) {
      const bayiIds = await getLoggedInBayiIds(); // birden fazla olabilir
      if (Array.isArray(bayiIds) && bayiIds.length > 0) {
        const set = new Set(bayiIds.map(String));
        mmler = (mmler || []).filter(mm => set.has(String(mm.BayiId)));
      } else {
        // oturumdan bayiId bulunamadıysa hiçbir şey göstermeyelim (isteğe göre değiştirilebilir)
        mmler = [];
      }
    }
    fillSelect(ddlMM, mmler, x => x.Id, x => x.AdSoyad || x.Unvan || '', true);
    grpMM && grpMM.classList.remove('d-none');
    if (ddlMM && ddlMM.options.length === 2) ddlMM.selectedIndex = 1;
  }
}

async function onRoleChange() {
  const rKey = roleKey(fmRol?.value || '');
  await loadLookupsForRole(rKey);
}

async function openModal(entity = null) {
  $('#userModalTitle') && ($('#userModalTitle').textContent = entity ? 'Kullanıcı Düzenle' : 'Yeni Kullanıcı');

  fmId.value = entity?.Id ?? '';
  fmAd.value = entity?.AdSoyad ?? '';
  fmMail.value = entity?.Eposta ?? '';
  fmPwd.value = entity?.Sifre ?? '';
  fmTel.value = entity?.Telefon ?? '';
  if (fmProf) fmProf.value = entity?.ProfilResmiUrl ?? '';
  formErr && (formErr.style.display = 'none');

  // Önce rol seçeneklerini, login rolüne göre kısıtla
  setRoleOptionsForLoggedIn();

  // Varsayılan seçim: Admin→Bayi, Bayi→Mali Müşavir; edit’te mevcut rol
  if (entity?.Rol && fmRol) {
    selectOneOf(fmRol, entity.Rol, roleKey(entity.Rol));
  } else if (fmRol) {
    if (isAdmin) selectOneOf(fmRol, 'Bayi', 'bayi');
    else if (isBayiUser) selectOneOf(fmRol, 'Mali Müşavir', 'MaliMusavir', 'malimusavir');
    else if (isMmUser) selectOneOf(fmRol, 'Firma', 'firma'); // MM için makul başlangıç
  }

  await onRoleChange(); // seçilen role göre lookup’lar hazırlanır
  modal?.show();
}

// ---------- kaydet ----------
async function save() {
  const id = val(fmId) || null;
  const rKey = roleKey(fmRol?.value || '');

  const basePayload = {
    Id: id || undefined,
    AdSoyad: val(fmAd),
    Eposta: val(fmMail),
    Sifre: fmPwd?.value || '',
    Telefon: val(fmTel),
    Rol: fmRol?.value || '',
    ProfilResmiUrl: fmProf ? val(fmProf) : null,
    OlusturanKullaniciId: sessionUserId,
    IsActive: true
  };

  if (!basePayload.AdSoyad || !basePayload.Eposta || !basePayload.Sifre) {
    formErr && (formErr.textContent = 'Ad Soyad, E-posta ve Şifre zorunludur.', formErr.style.display = 'block');
    return;
  }

  // 1) oluştur/güncelle
  let savedId = id;
  try {
    if (id) {
      await KullaniciApi.update(id, basePayload);
    } else {
      const createPayload = { ...basePayload, OlusturanKullaniciId: sessionUserId || null };
      const created = await KullaniciApi.create(createPayload);
      savedId = created?.Id;
    }
  } catch (err) {
    console.error('Kullanıcı kaydet hatası', err);
    alert(err?.payload?.title || err?.message || 'Kullanıcı kaydedilemedi.');
    return;
  }
  if (!savedId) { alert('Kullanıcı Id alınamadı.'); return; }

  // 2) Rol’e göre pivot
  try {
    if (rKey === 'bayi') {
      if (!isAdmin) {
        alert('Bayi rolü yalnızca admin tarafından atanabilir.');
      } else {
        const bayiId = val(ddlBayi);
        if (!bayiId) alert('Bayi seçimi zorunludur.');
        else {
          await KullaniciBayiApi.create({
            KullaniciId: savedId,
            BayiId: bayiId,
            IsPrimary: true,
            AtananRol: 'BayiAdmin',
            BaslangicTarihi: null,
            BitisTarihi: null
          });
        }
      }
    } else if (rKey === 'malimusavir') {
      const mmId = val(ddlMM);
      if (!mmId) {
        if (ddlMM && ddlMM.options.length <= 1) alert('Mali Müşavir bulunamadı. Önce tanımlayın.');
        else alert('Mali Müşavir seçimi zorunludur.');
      } else {
        await KullaniciMaliMusavirApi.create({
          KullaniciId: savedId,
          MaliMusavirId: mmId,
          IsPrimary: true,
          AtananRol: 'MMAdmin',
          BaslangicTarihi: null,
          BitisTarihi: null
        });
      }
    }
  } catch (err) {
    console.error('Pivot insert hatası', err);
    const msg = err?.payload?.title || err?.payload?.detail || err?.message || 'Kullanıcı ilişkilendirmesi yapılamadı.';
    alert(msg);
  }

  bootstrap.Modal.getInstance(modalEl)?.hide();
  toast('Kayıt kaydedildi', 'success');
  await load();
}

// ---------- bootstrap ----------
document.addEventListener('DOMContentLoaded', async () => {
  const s = await resolveSession();
  applySession(s);

  await load();

  $('#btnNewUser')?.addEventListener('click', () => openModal(null));
  $('#btnSaveUser')?.addEventListener('click', save);
  $('#txtSearch')?.addEventListener('input', applyFilter);
  $('#fRol')?.addEventListener('change', onRoleChange);

  $('#userTable')?.addEventListener('click', (e) => {
    const tr = e.target.closest('tr[data-id]'); if (!tr) return;
    const id = tr.getAttribute('data-id');
    if (e.target.closest('.btn-edit')) {
      const entity = rawUsers.find(x => x.Id === id) || all.find(x => x.Id === id);
      openModal(entity);
    } else if (e.target.closest('.btn-del')) {
      if (!confirm('Kullanıcı silinsin mi?')) return;
      KullaniciApi.remove(id).then(load);
    }
  });
});
