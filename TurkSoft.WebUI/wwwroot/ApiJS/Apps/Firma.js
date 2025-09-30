// Firma sayfası davranışı (liste + filtre + form modal)
// ======================================================================
import { FirmaApi, BayiApi, MaliMusavirApi } from '../entities/index.js';
import { getSession } from '../Service/LoginService.js';
// Basit yardımcılar
const $ = (sel) => document.querySelector(sel);
const $$ = (sel) => Array.from(document.querySelectorAll(sel));
const val = (el) => (el?.value ?? '').trim();

// DOM hazır
document.addEventListener('DOMContentLoaded', () => {
  // Elemanları yakala
  const tbody = $('#tbl-firma-body');         // tablo body
  const filterName = $('#fltFirmaAdi');         // filtre: firma adı
  const filterVergi = $('#fltVergiNo');          // filtre: vergi
  const filterBayi = $('#fltBayiId');           // filtre: bayi select
  const filterMM = $('#fltMaliMusavirId');    // filtre: mm select

  const btnNew = $('#btnNewFirma');             // yeni butonu
  const modalEl = $('#mdlFirma');               // modal
  const formEl = $('#frmFirma');               // form
  const modal = modalEl ? new bootstrap.Modal(modalEl) : null;

  // Form alanları
  const fId = $('#frmId');
  const fAdi = $('#frmFirmaAdi');
  const fVergi = $('#frmVergiNo');
  const fYet = $('#frmYetkiliAdSoyad');
  const fTel = $('#frmTelefon');
  const fEpo = $('#frmEposta');
  const fAdr = $('#frmAdres');
  const fBayi = $('#frmBayiId');
  const fMM = $('#frmMaliMusavirId');

  // Dropdown’ları doldur
  async function loadLookups() {
    const [bayiler, mmler] = await Promise.all([
      BayiApi.list(),
      MaliMusavirApi.list()
    ]);
    // Filtre dropdownları
    fillSelect(filterBayi, bayiler, 'Id', 'Unvan', true);
    fillSelect(filterMM, mmler, 'Id', 'AdSoyad', true);
    // Form dropdownları
    fillSelect(fBayi, bayiler, 'Id', 'Unvan', true);
    fillSelect(fMM, mmler, 'Id', 'AdSoyad', true);
  }

  function fillSelect(sel, items, valKey, textKey, addEmpty = false) {
    if (!sel) return;
    sel.innerHTML = '';
    if (addEmpty) {
      const o = document.createElement('option');
      o.value = ''; o.textContent = '— Seçiniz —';
      sel.appendChild(o);
    }
    (items || []).forEach(x => {
      const o = document.createElement('option');
      o.value = x[valKey]; o.textContent = x[textKey];
      sel.appendChild(o);
    });
  }

  // Listele (filtreli)
  async function loadTable() {
    const list = await FirmaApi.list(); // tümünü çek
    const name = val(filterName).toLowerCase();
    const vergi = val(filterVergi).toLowerCase();
    const bayiId = val(filterBayi);
    const mmId = val(filterMM);

    const filtered = (list || []).filter(x => {
      const okName = !name || (x.FirmaAdi || '').toLowerCase().includes(name);
      const okVerg = !vergi || (x.VergiNo || '').toLowerCase().includes(vergi);
      const okBayi = !bayiId || x.BayiId === bayiId;
      const okMM = !mmId || x.MaliMusavirId === mmId;
      return okName && okVerg && okBayi && okMM;
    });

    // Tablo bas
    tbody.innerHTML = (filtered || []).map(r => rowHtml(r)).join('');
    // Satır buton olayları
    bindRowActions();
  }

  function rowHtml(r) {
    return `
      <tr>
        <td>${r.FirmaAdi || ''}</td>
        <td>${r.VergiNo || ''}</td>
        <td>${r.YetkiliAdSoyad || ''}</td>
        <td>${r.Telefon || ''}</td>
        <td>${r.Eposta || ''}</td>
        <td>${r.Bayi?.Unvan || ''}</td>
        <td>${r.MaliMusavir?.AdSoyad || ''}</td>
        <td class="text-end">
          <button class="btn btn-sm btn-primary act-edit" data-id="${r.Id}">Düzenle</button>
          <button class="btn btn-sm btn-danger act-del"  data-id="${r.Id}">Sil</button>
        </td>
      </tr>`;
  }

  function bindRowActions() {
    $$('.act-edit').forEach(b => b.addEventListener('click', async (e) => {
      const id = e.currentTarget.getAttribute('data-id');
      const r = await FirmaApi.get(id);
      // formu doldur
      fId.value = r.Id;
      fAdi.value = r.FirmaAdi || '';
      fVergi.value = r.VergiNo || '';
      fYet.value = r.YetkiliAdSoyad || '';
      fTel.value = r.Telefon || '';
      fEpo.value = r.Eposta || '';
      fAdr.value = r.Adres || '';
      fBayi.value = r.BayiId || '';
      fMM.value = r.MaliMusavirId || '';
      modal?.show();
    }));

    $$('.act-del').forEach(b => b.addEventListener('click', async (e) => {
      const id = e.currentTarget.getAttribute('data-id');
      if (!confirm('Kaydı silmek istiyor musunuz?')) return;
      await FirmaApi.remove(id);
      await loadTable();
    }));
  }

  // Yeni butonu
  btnNew?.addEventListener('click', () => {
    formEl.reset();
    fId.value = '';
    modal?.show();
  });

  // Form submit
  formEl?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const dto = {
      Id: fId.value || undefined,
      FirmaAdi: fAdi.value,
      VergiNo: fVergi.value,
      YetkiliAdSoyad: fYet.value,
      Telefon: fTel.value,
      Eposta: fEpo.value,
      Adres: fAdr.value,
      BayiId: fBayi.value || null,
      MaliMusavirId: fMM.value || null
    };
    if (dto.Id) await FirmaApi.update(dto.Id, dto);
    else await FirmaApi.create(dto);
    modal?.hide();
    await loadTable();
  });

  // Filtre değişince listeyi yenile
  [filterName, filterVergi, filterBayi, filterMM].forEach(el => {
    el?.addEventListener('input', loadTable);
    el?.addEventListener('change', loadTable);
  });

  // İlk yükleme
  (async () => { await loadLookups(); await loadTable(); })();
});
