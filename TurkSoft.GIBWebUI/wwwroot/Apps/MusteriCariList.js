// wwwroot/js/pages/Lists/MusteriCariList.js
import { CustomerApi } from '../Entites/index.js';

(function (window, $) {
    'use strict';

    if (typeof $ === 'undefined') {
        console.error('[MusteriCariList] jQuery yok.');
        return;
    }

    /************************************************************
     *  RowVersion / BaseEntity yardımcıları
     ************************************************************/
    function rowVersionHexToBase64(hex) {
        if (!hex) return '';
        let h = String(hex).trim();
        if (h.startsWith('0x') || h.startsWith('0X')) {
            h = h.substring(2);
        }
        if (h.length % 2 === 1) {
            h = '0' + h;
        }
        const bytes = [];
        for (let i = 0; i < h.length; i += 2) {
            const b = parseInt(h.substr(i, 2), 16);
            if (!isNaN(b)) bytes.push(b);
        }
        let bin = '';
        for (let i = 0; i < bytes.length; i++) {
            bin += String.fromCharCode(bytes[i]);
        }
        try {
            return btoa(bin);
        } catch (e) {
            console.error('RowVersion base64 dönüşümünde hata:', e);
            return '';
        }
    }

    const DEFAULT_ROWVERSION_HEX = '0x00000000000007E1';
    const DEFAULT_ROWVERSION_BASE64 = rowVersionHexToBase64(DEFAULT_ROWVERSION_HEX);

    function getRowVersionFromEntityOrDefault(entity) {
        if (!entity) return DEFAULT_ROWVERSION_BASE64;

        let rv =
            entity.rowVersionBase64 || entity.RowVersionBase64 ||
            entity.rowVersion || entity.RowVersion ||
            entity.rowVersionHex || entity.RowVersionHex;

        if (rv && /^0x/i.test(rv)) {
            rv = rowVersionHexToBase64(rv);
        }

        if (!rv) {
            rv = DEFAULT_ROWVERSION_BASE64;
        }
        return rv;
    }

    /**
     * Customer için BaseEntity builder (swagger camelCase)
     */
    function buildBaseEntityForCustomer(isNew, nowIso, existing, overrideRowVersion) {
        const ex = existing || {};
        const BASE_USER_ID = getCurrentUserIdForGib(); // her zaman güncel kullanıcı

        if (isNew) {
            return {
                userId: BASE_USER_ID,
                isActive: true,
                deleteDate: null,
                deletedByUserId: null,
                createdAt: nowIso,
                updatedAt: nowIso,
                createdByUserId: BASE_USER_ID,
                updatedByUserId: BASE_USER_ID,
                rowVersion: overrideRowVersion || DEFAULT_ROWVERSION_BASE64
            };
        }

        const createdAt = ex.createdAt || ex.CreatedAt || nowIso;

        const isActive =
            (ex.isActive !== undefined ? ex.isActive :
                (ex.IsActive !== undefined ? ex.IsActive : true));

        const deleteDate = ex.deleteDate || ex.DeleteDate || null;
        const deletedByUserId = ex.deletedByUserId || ex.DeletedByUserId || null;

        const createdByUserId =
            ex.createdByUserId || ex.CreatedByUserId || BASE_USER_ID;

        const updatedByUserId =
            ex.updatedByUserId || ex.UpdatedByUserId || BASE_USER_ID;

        const userId =
            ex.userId || ex.UserId || BASE_USER_ID;

        let rowVersionBase64 = overrideRowVersion;
        if (!rowVersionBase64) {
            rowVersionBase64 = getRowVersionFromEntityOrDefault(ex);
        }

        return {
            userId: userId,
            isActive: isActive,
            deleteDate: deleteDate,
            deletedByUserId: deletedByUserId,
            createdAt: createdAt,
            updatedAt: nowIso,
            createdByUserId: createdByUserId,
            updatedByUserId: updatedByUserId,
            rowVersion: rowVersionBase64
        };
    }

    /************************************************************
     *  Küçük yardımcılar
     ************************************************************/
    function html(s) {
        return String(s == null ? '' : s)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    /************************************************************
     *  Global state
     ************************************************************/
    let table = null;
    let rows = [];
    let rawCustomersById = {};   // id → API entity
    let currentCustomerEntity = null;

    /************************************************************
     *  API → GRID normalize (Customer JSON → grid modeli)
     ************************************************************/
    function normalizeApiCustomer(raw) {
        raw = raw || {};

        const id = (raw.id != null) ? raw.id : raw.Id;
        const name = (raw.name != null) ? raw.name : raw.Name;
        const surname = (raw.surname != null) ? raw.surname : raw.Surname;
        const phone = (raw.phone != null) ? raw.phone : raw.Phone;
        const email = (raw.email != null) ? raw.email : raw.Email;
        const taxNo = (raw.taxNo != null) ? raw.taxNo : raw.TaxNo;
        const taxOffice = (raw.taxOffice != null) ? raw.taxOffice : raw.TaxOffice;

        const createdAt = raw.createdAt || raw.CreatedAt || null;
        const updatedAt = raw.updatedAt || raw.UpdatedAt || null;

        return {
            Id: id || 0,
            Name: name || '',
            Surname: surname || '',
            Phone: phone || '',
            Email: email || '',
            TaxNo: taxNo || '',
            TaxOffice: taxOffice || '',
            CreatedAt: createdAt,
            UpdatedAt: updatedAt,
            CreatedAtText: createdAt ? new Date(createdAt).toLocaleString('tr-TR') : '',
            UpdatedAtText: updatedAt ? new Date(updatedAt).toLocaleString('tr-TR') : ''
        };
    }

    /************************************************************
     *  DataTable
     ************************************************************/
    function initTable() {
        const $tbl = $('#gridCari');
        if (!$tbl.length) {
            console.warn('[MusteriCariList] #gridCari bulunamadı.');
            return;
        }

        table = $tbl.DataTable({
            data: [],
            pageLength: 25,
            order: [[0, 'asc']],
            columns: [
                { data: 'Id', title: 'Müşteri Kodu' },
                { data: 'TaxNo', title: 'VKN/TCKN' },
                { data: 'TaxOffice', title: 'Vergi Dairesi' },
                { data: 'Name', title: 'Ad / Ünvan' },
                { data: 'Surname', title: 'Soyad' },
                { data: 'Phone', title: 'Telefon' },
                { data: 'Email', title: 'E-posta' },
                {
                    data: 'CreatedAtText',
                    title: 'Oluşturma Tarihi'
                },
                {
                    data: null,
                    orderable: false,
                    className: 'text-center',
                    render: function (row) {
                        return '' +
                            `<button class="btn btn-xs btn-primary act-edit" data-id="${html(row.Id)}"><i class="fa fa-edit"></i></button> ` +
                            `<button class="btn btn-xs btn-danger act-del" data-id="${html(row.Id)}"><i class="fa fa-trash"></i></button>`;
                    }
                }
            ],
            language: { url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/tr.json' }
        });
    }

    function refreshTable() {
        if (!table) return;
        table.clear().rows.add(rows).draw();
    }

    /************************************************************
     *  API'den liste (UserId filtresi ile)
     ************************************************************/
    async function loadCustomers() {
        try {
            const userId = getCurrentUserIdForGib();
            const data = await CustomerApi.list();

            rawCustomersById = {};
            const filtered = (data || []).filter(x => {
                const u = (x.userId != null) ? x.userId : x.UserId;
                return !userId || u === userId;
            });

            filtered.forEach(c => {
                const id = (c.id != null) ? c.id : c.Id;
                if (id != null) {
                    rawCustomersById[id] = c;
                }
            });

            rows = filtered.map(normalizeApiCustomer);
            refreshTable();
        } catch (err) {
            console.error('[MusteriCariList] listeleme hatası:', err);
            alert('Müşteri/Cari listesi alınamadı.');
        }
    }

    /************************************************************
     *  Modal açma
     ************************************************************/
    function openModal(entity) {
        currentCustomerEntity = entity || null;

        const raw = entity || {};
        const id = raw.id ?? raw.Id ?? 0;

        const name = raw.name ?? raw.Name ?? '';
        const surname = raw.surname ?? raw.Surname ?? '';
        const phone = raw.phone ?? raw.Phone ?? '';
        const email = raw.email ?? raw.Email ?? '';
        const taxNo = raw.taxNo ?? raw.TaxNo ?? '';
        const taxOffice = raw.taxOffice ?? raw.TaxOffice ?? '';

        const createdAt = raw.createdAt || raw.CreatedAt || null;
        const updatedAt = raw.updatedAt || raw.UpdatedAt || null;

        const $modal = $('#mdlCustomer');
        const $form = $('#frmCustomer')[0];

        if ($form && $form.reset) $form.reset();

        $('#cusId').val(id || '');
        $('#cusName').val(name);
        $('#cusSurname').val(surname);
        $('#cusPhone').val(phone);
        $('#cusEmail').val(email);
        $('#cusTaxNo').val(taxNo);
        $('#cusTaxOffice').val(taxOffice);

        const todayStr = new Date().toISOString().split('T')[0];

        $('#cusCreatedAt').val(
            createdAt ? new Date(createdAt).toISOString().split('T')[0] : todayStr
        );
        $('#cusUpdatedAt').val(
            updatedAt ? new Date(updatedAt).toISOString().split('T')[0] : todayStr
        );

        $modal.modal('show');
    }

    /************************************************************
     *  Kaydet (create / update) – swagger Customer JSON formatına göre
     ************************************************************/
    async function saveFromForm() {
        const form = $('#frmCustomer')[0];
        if (form && form.checkValidity && !form.checkValidity()) {
            form.reportValidity && form.reportValidity();
            return;
        }

        const id = parseInt($('#cusId').val(), 10) || 0;
        const isNew = !id;

        const name = $('#cusName').val().trim();
        const surname = $('#cusSurname').val().trim();
        const phone = $('#cusPhone').val().trim();
        const email = $('#cusEmail').val().trim();
        const taxNo = $('#cusTaxNo').val().trim();
        const taxOffice = $('#cusTaxOffice').val().trim();

        if (!taxNo || ![10, 11].includes(taxNo.length)) {
            alert('VKN/TCKN 10 veya 11 haneli olmalıdır.');
            return;
        }
        if (!name) {
            alert('Ad/Ünvan zorunludur.');
            return;
        }

        const createdAtVal = $('#cusCreatedAt').val();
        const updatedAtVal = $('#cusUpdatedAt').val();

        const createdAtIso = createdAtVal
            ? new Date(createdAtVal).toISOString()
            : new Date().toISOString();

        const nowIso = new Date().toISOString();

        const baseEntity = buildBaseEntityForCustomer(
            isNew,
            nowIso,
            isNew ? null : currentCustomerEntity,
            null
        );

        // Mevcut ilişkiler (UI’de şimdilik yok, boş gönderiyoruz)
        const existing = currentCustomerEntity || {};
        const customersGroups =
            existing.customersGroups || existing.CustomersGroups || [];
        const addresses =
            existing.addresses || existing.Addresses || [];

        const dto = {
            ...baseEntity,

            id: isNew ? 0 : id,
            name: name,
            surname: surname,
            phone: phone || '-',
            email: email || '',
            taxNo: taxNo,
            taxOffice: taxOffice || '',

            customersGroups: customersGroups || [],
            addresses: addresses || [],
            invoices: [] // Customer ekranından invoice üretmiyoruz
        };

        // createdAt’i formdan gelen değerle override edelim (yeni kayıt ise now da olabilir)
        dto.createdAt = createdAtIso;

        try {
            if (isNew) {
                await CustomerApi.create(dto);
            } else {
                await CustomerApi.update(dto.id, dto);
            }

            $('#mdlCustomer').modal('hide');
            await loadCustomers();
        } catch (err) {
            console.error('[MusteriCariList] kaydetme hatası:', err);
            alert('Müşteri kaydedilemedi.');
        }
    }

    /************************************************************
     *  Event binding
     ************************************************************/
    function bindEvents() {
        // Yeni
        $('#btnNewCustomer').on('click', function () {
            openModal(null);
        });

        // Kaydet (form submit)
        $('#frmCustomer').on('submit', function (e) {
            e.preventDefault();
            saveFromForm();
        });

        // Edit
        $(document).on('click', '.act-edit', async function () {
            const id = parseInt($(this).data('id'), 10);
            if (!id) return;

            try {
                const entity = await CustomerApi.get(id);
                openModal(entity);
            } catch (err) {
                console.error('[MusteriCariList] get hatası:', err);
                alert('Müşteri kaydı getirilemedi.');
            }
        });

        // Sil
        $(document).on('click', '.act-del', async function () {
            const id = parseInt($(this).data('id'), 10);
            if (!id) return;
            if (!confirm('Bu müşteri/cari kaydı silinsin mi?')) return;

            try {
                await CustomerApi.remove(id);
                await loadCustomers();
            } catch (err) {
                console.error('[MusteriCariList] silme hatası:', err);
                alert('Kayıt silinemedi.');
            }
        });
    }

    /************************************************************
     *  UserId helper (Item sayfasıyla aynı mantık)
     ************************************************************/
    function getCurrentUserIdForGib() {
        let userId = 0;

        // 1) Hidden input
        const $userHidden = $('#hdnUserId');
        if ($userHidden.length) {
            const parsed = parseInt($userHidden.val(), 10);
            if (!isNaN(parsed) && parsed > 0) {
                userId = parsed;
            }
        }

        // 2) window.currentUserId
        if (!userId && typeof window.currentUserId === 'number' && window.currentUserId > 0) {
            userId = window.currentUserId;
        }

        // 3) sessionStorage
        if (!userId && typeof sessionStorage !== 'undefined') {
            try {
                const stored =
                    sessionStorage.getItem('CurrentUserId') ||
                    sessionStorage.getItem('currentUserId') ||
                    sessionStorage.getItem('UserId');

                if (stored) {
                    const parsed = parseInt(stored, 10);
                    if (!isNaN(parsed) && parsed > 0) {
                        userId = parsed;
                        console.log('[MusteriCariList] userId sessionStorage\'dan okundu:', userId);
                    }
                }
            } catch (e) {
                console.warn('[MusteriCariList] CurrentUserId sessionStorage\'dan okunamadı:', e);
            }
        }

        if (!userId) {
            throw new Error('Kullanıcı Id (userId) bulunamadı. Lütfen oturumu kontrol edin.');
        }

        return userId;
    }

    /************************************************************
     *  INIT
     ************************************************************/
    $(function () {
        if (!$('#gridCari').length) {
            return;
        }

        try {
            getCurrentUserIdForGib();
        } catch (e) {
            console.error('[MusteriCariList] userId alınamadı:', e);
            alert('Kullanıcı oturumu bulunamadı (userId).');
            return;
        }

        initTable();
        bindEvents();
        loadCustomers();
    });

})(window, jQuery);
