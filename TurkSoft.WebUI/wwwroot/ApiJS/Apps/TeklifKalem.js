import { TeklifKalemApi, LogApi } from '../entities/index.js';
import { getSession } from '../Service/LoginService.js';

const $ = s => document.querySelector(s);
const $$ = s => Array.from(document.querySelectorAll(s));
const val = el => (el?.value ?? '').trim();

document.addEventListener('DOMContentLoaded', async () => {
  const tbody = $('#tbl-teklifkalem-body');
  const btnNew = $('#btnNewTeklifKalem');
  const modalEl = $('#mdlTeklifKalem');
  const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
  const formEl = $('#frmTeklifKalem');

  // Form alanları
  const fmId = $('#frmId');
  const fmTeklifId = $('#frmTeklifId');
  const fmPaketId = $('#frmPaketId');
  const fmMiktar = $('#frmMiktar');
  const fmBirimFiyat = $('#frmBirimFiyat');
  const fmTutar = $('#frmTutar');

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
        Islem: `${userId}-TeklifKalem-${tip}`,
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
    const list = await TeklifKalemApi.list();
    await logIslem("List");

    tbody.innerHTML = (Array.isArray(list) ? list : []).map(r => `
      <tr>
        <td>${r.Id}</td>
        <td>${r.Miktar ?? ''}</td>
        <td>${r.Tutar ?? 0}</td>
        <td class="text-end">
          <button class="btn btn-sm btn-primary act-edit" data-id="${r.Id}">Düzenle</button>
          <button class="btn btn-sm btn-danger act-del" data-id="${r.Id}">Sil</button>
        </td>
      </tr>`).join('');

    bindRowActions();
  }

  // ---- Tek kayıt getirme ----
  async function getById(id) {
    const r = await TeklifKalemApi.get(id);
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
        fmTeklifId.value = r.TeklifId ?? '';
        fmPaketId.value = r.PaketId ?? '';
        fmMiktar.value = r.Miktar ?? 0;
        fmBirimFiyat.value = r.BirimFiyat ?? 0;
        fmTutar.value = r.Tutar ?? 0;

        modal?.show();
      })
    );

    $$('.act-del').forEach(b =>
      b.addEventListener('click', async e => {
        const id = e.currentTarget.getAttribute('data-id');
        if (!confirm('Silinsin mi?')) return;
        await TeklifKalemApi.remove(id);
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
      TeklifId: val(fmTeklifId),
      PaketId: val(fmPaketId),
      Miktar: parseInt(val(fmMiktar)) || 0,
      BirimFiyat: parseFloat(val(fmBirimFiyat)) || 0,
      Tutar: parseFloat(val(fmTutar)) || 0
    };

    if (dto.Id) {
      await TeklifKalemApi.update(dto.Id, dto);
      await logIslem("Update");
    } else {
      await TeklifKalemApi.create(dto);
      await logIslem("Insert");
    }

    modal?.hide();
    await loadTable();
  });

  await loadTable();
});
