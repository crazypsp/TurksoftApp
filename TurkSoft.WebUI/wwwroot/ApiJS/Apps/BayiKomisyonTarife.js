import { BayiKomisyonTarifeApi, LogApi } from '../entities/index.js';
import { getSession } from '../Service/LoginService.js';
const $ = s => document.querySelector(s);
const $$ = s => Array.from(document.querySelectorAll(s));
const val = el => (el?.value ?? '').trim();

document.addEventListener('DOMContentLoaded', async () => {
  const tbody = $('#tbl-bayikomisyontarife-body');
  const btnNew = $('#btnNewBayiKomisyonTarife');
  const modalEl = $('#mdlBayiKomisyonTarife');
  const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
  const formEl = $('#frmBayiKomisyonTarife');

  // Form alanları
  const fmId = $('#frmId');
  const fmBayiId = $('#frmBayiId');
  const fmPaketId = $('#frmPaketId');
  const fmKomisyonYuzde = $('#frmKomisyonYuzde');
  const fmBaslangic = $('#frmBaslangic');
  const fmBitis = $('#frmBitis');

  // ---- Log ekleme helper ----
  async function logIslem(tip) {
    try {
      const session = await getSession(); // { Id, ... }
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
    const list = await BayiKomisyonTarifeApi.list();
    await logIslem("List");
    tbody.innerHTML = (Array.isArray(list) ? list : []).map(r => `
      <tr>
        <td>${r.Id}</td>
        <td>${r.BayiId ?? ''}</td>
        <td>${r.PaketId ?? ''}</td>
        <td>${r.KomisyonYuzde ?? ''}</td>
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
    const r = await BayiKomisyonTarifeApi.get(id);
    await logIslem("GetById");
    return r;
  }

  function bindRowActions() {
    $$('.act-edit').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      const r = await getById(id);
      fmId.value = r.Id;
      fmBayiId.value = r.BayiId ?? '';
      fmPaketId.value = r.PaketId ?? '';
      fmKomisyonYuzde.value = r.KomisyonYuzde ?? '';
      fmBaslangic.value = r.Baslangic ?? '';
      fmBitis.value = r.Bitis ?? '';
      modal?.show();
    }));
    $$('.act-del').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      if (!confirm('Silinsin mi?')) return;
      await BayiKomisyonTarifeApi.remove(id);
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
      BayiId: val(fmBayiId),
      PaketId: val(fmPaketId),
      KomisyonYuzde: parseFloat(val(fmKomisyonYuzde)) || 0,
      Baslangic: val(fmBaslangic),
      Bitis: val(fmBitis) || null
    };

    if (dto.Id) {
      await BayiKomisyonTarifeApi.update(dto.Id, dto);
      await logIslem("Update");
    } else {
      await BayiKomisyonTarifeApi.create(dto);
      await logIslem("Insert");
    }

    modal?.hide();
    await loadTable();
  });

  await loadTable();
});
