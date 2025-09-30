import { SistemBildirimApi, LogApi } from '../entities/index.js';
import { getSession } from '../Service/LoginService.js';

const $ = s => document.querySelector(s);
const $$ = s => Array.from(document.querySelectorAll(s));
const val = el => (el?.value ?? '').trim();

document.addEventListener('DOMContentLoaded', async () => {
  const tbody = $('#tbl-sistembildirim-body');
  const btnNew = $('#btnNewSistemBildirim');
  const modalEl = $('#mdlSistemBildirim');
  const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
  const formEl = $('#frmSistemBildirim');

  // Form alanları
  const fmId = $('#frmId');
  const fmKanal = $('#frmKanal');
  const fmPlanlananTarih = $('#frmPlanlananTarih');
  const fmBaslik = $('#frmBaslik');
  const fmIcerik = $('#frmIcerik');
  const fmDurum = $('#frmDurum');
  const fmHedefKullaniciId = $('#frmHedefKullaniciId');

  // ---- Log helper ----
  async function logIslem(tip) {
    try {
      const session = await getSession();
      const userId = session?.Id || "0";
      const ipAdres = await fetch("https://api.ipify.org?format=json")
        .then(r => r.json())
        .then(d => d.ip)
        .catch(() => "unknown");

      const dto = {
        Islem: `${userId}-SistemBildirim-${tip}`,
        IpAdres: ipAdres,
        Tarayici: navigator.userAgent
      };
      await LogApi.create(dto);
    } catch (err) {
      console.error("Log kaydı başarısız:", err);
    }
  }

  // ---- Listeleme ----
  async function loadTable() {
    const list = await SistemBildirimApi.list();
    await logIslem("List");

    tbody.innerHTML = (Array.isArray(list) ? list : []).map(r => `
      <tr>
        <td>${r.Id}</td>
        <td>${r.Kanal ?? ''}</td>
        <td>${r.PlanlananTarih ? new Date(r.PlanlananTarih).toLocaleString() : ''}</td>
        <td>${r.Baslik ?? ''}</td>
        <td>${r.Durum ?? ''}</td>
        <td>${r.HedefKullaniciId ?? ''}</td>
        <td class="text-end">
          <button class="btn btn-sm btn-primary act-edit" data-id="${r.Id}">Düzenle</button>
          <button class="btn btn-sm btn-danger act-del" data-id="${r.Id}">Sil</button>
        </td>
      </tr>`).join('');

    bindRowActions();
  }

  // ---- Tek kayıt getirme ----
  async function getById(id) {
    const r = await SistemBildirimApi.get(id);
    await logIslem("GetById");
    return r;
  }

  // ---- Satır aksiyonları ----
  function bindRowActions() {
    $$('.act-edit').forEach(b =>
      b.addEventListener('click', async e => {
        const id = e.currentTarget.getAttribute('data-id');
        const r = await getById(id);

        fmId.value = r.Id;
        fmKanal.value = r.Kanal ?? '';
        fmPlanlananTarih.value = r.PlanlananTarih ? new Date(r.PlanlananTarih).toISOString().slice(0, 16) : '';
        fmBaslik.value = r.Baslik ?? '';
        fmIcerik.value = r.Icerik ?? '';
        fmDurum.value = r.Durum ?? '';
        fmHedefKullaniciId.value = r.HedefKullaniciId ?? '';

        modal?.show();
      })
    );

    $$('.act-del').forEach(b =>
      b.addEventListener('click', async e => {
        const id = e.currentTarget.getAttribute('data-id');
        if (!confirm('Silinsin mi?')) return;
        await SistemBildirimApi.remove(id);
        await logIslem("Delete");
        await loadTable();
      })
    );
  }

  // ---- Yeni kayıt ----
  btnNew?.addEventListener('click', () => {
    formEl.reset();
    fmId.value = '';
    modal?.show();
  });

  // ---- Insert / Update ----
  formEl?.addEventListener('submit', async e => {
    e.preventDefault();

    const dto = {
      Id: fmId.value || undefined,
      Kanal: parseInt(val(fmKanal)) || 0,
      PlanlananTarih: val(fmPlanlananTarih) || null,
      Baslik: val(fmBaslik),
      Icerik: val(fmIcerik),
      Durum: parseInt(val(fmDurum)) || 0,
      HedefKullaniciId: val(fmHedefKullaniciId) || null
    };

    if (dto.Id) {
      await SistemBildirimApi.update(dto.Id, dto);
      await logIslem("Update");
    } else {
      await SistemBildirimApi.create(dto);
      await logIslem("Insert");
    }

    modal?.hide();
    await loadTable();
  });

  await loadTable();
});
