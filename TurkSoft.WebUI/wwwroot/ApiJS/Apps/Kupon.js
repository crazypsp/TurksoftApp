import { KuponApi, LogApi } from '../entities/index.js';
import { getSession } from '../Service/LoginService.js';
const $ = s => document.querySelector(s);
const $$ = s => Array.from(document.querySelectorAll(s));
const val = el => (el?.value ?? '').trim();

document.addEventListener('DOMContentLoaded', async () => {
  const tbody = $('#tbl-kupon-body');
  const btnNew = $('#btnNewKupon');
  const modalEl = $('#mdlKupon');
  const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
  const formEl = $('#frmKupon');

  // Form alanları
  const fmId = $('#frmId');
  const fmKod = $('#frmKod');
  const fmBayiId = $('#frmBayiId');
  const fmIndirimYuzde = $('#frmIndirimYuzde');
  const fmIndirimTutar = $('#frmIndirimTutar');
  const fmMaksKullanim = $('#frmMaksKullanim');
  const fmKullanildi = $('#frmKullanildi');
  const fmBaslangic = $('#frmBaslangic');
  const fmBitis = $('#frmBitis');

  // ---- Log ekleme helper ----
  async function logIslem(tip) {
    try {
      const session = await getSession();
      const userId = session?.Id || "0";
      const ipAdres = await fetch("https://api.ipify.org?format=json")
        .then(r => r.json())
        .then(d => d.ip)
        .catch(() => "unknown");

      const dto = {
        Islem: `${userId}-${tip}`,
        IpAdres: ipAdres,
        Tarayici: navigator.userAgent
      };
      await LogApi.create(dto);
    } catch (err) {
      console.error("Log yazılamadı:", err);
    }
  }

  // ---- Listeleme ----
  async function loadTable() {
    const list = await KuponApi.list();
    await logIslem("List");
    tbody.innerHTML = (Array.isArray(list) ? list : []).map(r => `
      <tr>
        <td>${r.Id}</td>
        <td>${r.Kod ?? ''}</td>
        <td>${r.BayiId ?? ''}</td>
        <td>${r.IndirimYuzde ?? ''}</td>
        <td>${r.IndirimTutar ?? ''}</td>
        <td>${r.MaksKullanim ?? ''}</td>
        <td>${r.Kullanildi ?? ''}</td>
        <td>${r.Baslangic ?? ''}</td>
        <td>${r.Bitis ?? ''}</td>
        <td class="text-end">
          <button class="btn btn-sm btn-primary act-edit" data-id="${r.Id}">Düzenle</button>
          <button class="btn btn-sm btn-danger act-del"  data-id="${r.Id}">Sil</button>
        </td>
      </tr>`).join('');
    bindRowActions();
  }

  // ---- Tek kayıt getirme ----
  async function getById(id) {
    const r = await KuponApi.get(id);
    await logIslem("GetById");
    return r;
  }

  function bindRowActions() {
    $$('.act-edit').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      const r = await getById(id);
      fmId.value = r.Id;
      fmKod.value = r.Kod ?? '';
      fmBayiId.value = r.BayiId ?? '';
      fmIndirimYuzde.value = r.IndirimYuzde ?? '';
      fmIndirimTutar.value = r.IndirimTutar ?? '';
      fmMaksKullanim.value = r.MaksKullanim ?? '';
      fmKullanildi.value = r.Kullanildi ?? '';
      fmBaslangic.value = r.Baslangic ?? '';
      fmBitis.value = r.Bitis ?? '';
      modal?.show();
    }));
    $$('.act-del').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      if (!confirm('Silinsin mi?')) return;
      await KuponApi.remove(id);
      await logIslem("Delete");
      await loadTable();
    }));
  }

  // ---- Yeni kayıt butonu ----
  btnNew?.addEventListener('click', () => {
    formEl.reset();
    fmId.value = '';
    modal?.show();
  });

  // ---- Kayıt ekleme / güncelleme ----
  formEl?.addEventListener('submit', async e => {
    e.preventDefault();
    const dto = {
      Id: fmId.value || undefined,
      Kod: val(fmKod),
      BayiId: val(fmBayiId) || null,
      IndirimYuzde: parseFloat(val(fmIndirimYuzde)) || null,
      IndirimTutar: parseFloat(val(fmIndirimTutar)) || null,
      MaksKullanim: parseInt(val(fmMaksKullanim)) || 0,
      Kullanildi: parseInt(val(fmKullanildi)) || 0,
      Baslangic: val(fmBaslangic),
      Bitis: val(fmBitis) || null
    };

    if (dto.Id) {
      await KuponApi.update(dto.Id, dto);
      await logIslem("Update");
    } else {
      await KuponApi.create(dto);
      await logIslem("Insert");
    }

    modal?.hide();
    await loadTable();
  });

  await loadTable();
});
