// wwwroot/ApiJS/Apps/Kullanici.js
// ======================================================================
// Bu modül, Kullanıcı Index.cshtml sayfasını sürer:
//  - Listeyi çeker, tabloya basar (DataTables varsa entegre olur).
//  - Arama kutusu filtreler.
//  - Yeni/Düzenle modalını açar, kaydeder (create/update).
//  - Silme işlemini yapar.
// ======================================================================

import { KullaniciApi } from '../entities/index.js';

let all = [];              // son çekilen liste (cache)
let dt = null;            // DataTables örneği (varsa)

function $(sel) { return document.querySelector(sel); }
function $$(sel) { return document.querySelectorAll(sel); }

function toast(msg, type = 'success') {
  // Basit: istersen kendi toast sisteminle değiştir
  console.log(`[${type}]`, msg);
}

// === Tabloyu doldur ===
function renderTable(items) {
  const tbody = $('#userTable tbody');
  tbody.innerHTML = items.map(u => `
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

  // DataTables varsa initialize et
  if (window.DataTable) {
    if (dt) dt.destroy();
    dt = new window.DataTable('#userTable', {
      responsive: true,
      searching: false, // kendi aramamızı kullanıyoruz
      lengthChange: false
    });
  }
}

// === Kart istatistiklerini güncelle ===
function renderStats(items) {
  $('#statTotal').textContent = items.length;
  const admins = items.filter(x => (x.Rol || '').toLowerCase() === 'admin').length;
  $('#statAdmins').textContent = admins;
  $('#statOther').textContent = items.length - admins;
}

// === Sunucudan çek ===
async function load() {
  const data = await KullaniciApi.list();
  all = Array.isArray(data) ? data : [];
  renderTable(all);
  renderStats(all);
}

// === Arama ===
function applyFilter() {
  const q = ($('#txtSearch').value || '').toLowerCase().trim();
  const filtered = !q ? all : all.filter(u => {
    const hay = `${u.AdSoyad ?? ''} ${u.Eposta ?? ''} ${u.Rol ?? ''}`.toLowerCase();
    return hay.includes(q);
  });
  renderTable(filtered);
}

// === Modal aç/doldur ===
function openModal(entity = null) {
  $('#userModalTitle').textContent = entity ? 'Kullanıcı Düzenle' : 'Yeni Kullanıcı';
  $('#userId').value = entity?.Id ?? '';
  $('#fAdSoyad').value = entity?.AdSoyad ?? '';
  $('#fEposta').value = entity?.Eposta ?? '';
  $('#fSifre').value = entity?.Sifre ?? '';
  $('#fTelefon').value = entity?.Telefon ?? '';
  $('#fRol').value = entity?.Rol ?? 'Bayi';
  $('#fProfil').value = entity?.ProfilResmiUrl ?? '';
  $('#formError').style.display = 'none';

  const modal = new bootstrap.Modal(document.getElementById('userModal'));
  modal.show();
}

// === Kaydet (create/update) ===
async function save() {
  const id = $('#userId').value || null;
  const payload = {
    Id: id || undefined,
    AdSoyad: $('#fAdSoyad').value.trim(),
    Eposta: $('#fEposta').value.trim(),
    Sifre: $('#fSifre').value,        // not: canlıda hash
    Telefon: $('#fTelefon').value.trim(),
    Rol: $('#fRol').value,
    ProfilResmiUrl: $('#fProfil').value.trim(),
    IsActive: true
  };

  // basit doğrulama
  if (!payload.AdSoyad || !payload.Eposta || !payload.Sifre) {
    const box = $('#formError'); box.textContent = 'Ad Soyad, E-posta ve Şifre zorunludur.'; box.style.display = 'block';
    return;
  }

  if (id) await KullaniciApi.update(id, payload);
  else await KullaniciApi.create(payload);

  bootstrap.Modal.getInstance(document.getElementById('userModal')).hide();
  toast('Kayıt kaydedildi');
  await load();
}

// === Sil ===
async function remove(id) {
  if (!confirm('Kullanıcı silinsin mi?')) return;
  await KullaniciApi.remove(id);
  toast('Kayıt silindi', 'warning');
  await load();
}

// === Event binding ===
document.addEventListener('DOMContentLoaded', async () => {
  await load();

  // arama
  $('#txtSearch').addEventListener('input', applyFilter);

  // yeni
  $('#btnNewUser').addEventListener('click', () => openModal(null));

  // kaydet
  $('#btnSaveUser').addEventListener('click', save);

  // satır butonları
  document.getElementById('userTable').addEventListener('click', (e) => {
    const tr = e.target.closest('tr[data-id]');
    if (!tr) return;
    const id = tr.getAttribute('data-id');
    if (e.target.closest('.btn-edit')) {
      const entity = all.find(x => x.Id === id);
      openModal(entity);
    } else if (e.target.closest('.btn-del')) {
      remove(id);
    }
  });
});
