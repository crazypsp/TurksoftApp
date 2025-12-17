/*! turkcellEfaturaApi.js
 * ------------------------------------------------------------
 * Bu dosya 2 farklı API base’i aynı anda kullanır:
 *  - api-base              (Api:BaseUrl)             => genel/ERP ekran endpointleri
 *  - gib-portal-api-base   (Api:GibPortalApiBaseUrl) => TurkcellEFaturaController (GİB portal)
 *
 * View modülleri named export beklediği için hepsi export edilmiştir:
 *  - turkcellEfaturaApi
 *  - EinvoiceInboxApi
 *  - EinvoiceOutboxApi
 *  - EinvoiceTransferApi
 *  - EarchiveOutboxApi
 *  - EarchiveTransferApi
 *  - EinvoiceGibPortalApi
 */

import { BaseTurkcellEfaturaApiService } from "./BaseTurkcellEfaturaApiService.js";

/** Route’ları tek yerden yönet: Backend’de path farklıysa sadece burayı güncelle. */
const ROUTES = {
    // ---- GİB PORTAL (TurkcellEFaturaController) ----
    gib: {
        resource: "TurkcellEFatura",

        static: {
            unit: "static/unit",
            taxExemption: "static/taxexemption",
            withholding: "static/withholding",
            taxTypeCode: "static/taxtypecode",
            taxOffice: "static/taxoffice",
            country: "static/country"
        },

        balance: {
            get: "balance",
            consume: "balance/consume"
        },

        einvoice: {
            // Inbox
            inboxList: "einvoice/inbox/list",
            inboxPdf: (ettn) => `einvoice/inbox/pdf/${ettn}`,
            inboxUbl: (ettn) => `einvoice/inbox/ubl/${ettn}`,
            inboxHtml: (ettn) => `einvoice/inbox/html/${ettn}`,
            inboxStatus: (ettn) => `einvoice/inbox/status/${ettn}`,
            inboxIsNewUpdate: "einvoice/inbox/isnew",

            // Outbox (Controller’ında bazıları yoksa backend ekleyince çalışır)
            outboxList: "einvoice/outbox/list",
            outboxPdf: (ettn) => `einvoice/outbox/pdf/${ettn}`,
            outboxUbl: (ettn) => `einvoice/outbox/ubl/${ettn}`,
            outboxHtml: (ettn) => `einvoice/outbox/html/${ettn}`,
            outboxStatus: (ettn) => `einvoice/outbox/status/${ettn}`,
            outboxReason: (ettn) => `einvoice/outbox/reason/${ettn}`,
            outboxWithNullLocalRef: "einvoice/outbox/with-null-localref",
            outboxUpdateUbl: "einvoice/outbox/update-ubl",
            outboxUpdateJson: (ettn) => `einvoice/outbox/update-json/${ettn}`,
            outboxStatusList: "einvoice/outbox/statuslist",

            // Send
            sendJson: (invoiceId) => `einvoice/send-json/${invoiceId}`,
            sendUbl: "einvoice/send-ubl",

            // Responses
            sendResponse: "einvoice/response",
            retryResponseList: "einvoice/response/retry"
        },

        earchive: {
            sendJson: (invoiceId) => `earchive/send-json/${invoiceId}`,
            sendJsonRaw: "earchive/send-json-raw",
            sendUbl: "earchive/send-ubl",
            updateUbl: "earchive/update-ubl",
            pdf: (ettn) => `earchive/pdf/${ettn}`,
            ubl: (ettn) => `earchive/ubl/${ettn}`,
            html: (ettn) => `earchive/html/${ettn}`,
            status: (ettn) => `earchive/status/${ettn}`,
            withNullLocalRef: "earchive/with-null-localref",
            cancel: "earchive/cancel",
            retryMail: (id) => `earchive/retry-mail/${id}`,
            retryMailCustom: "earchive/retry-mail-custom"
        },

        gibuser: {
            recipientZip: "gibuser/recipient-zip"
        }
    },

    // ---- “GENEL/ERP” API (api-base) – UI ekranlarının çoğu burada ----
    // Not: Bunlar view scriptlerinden türetilen isimlendirmedir; backend’in farklıysa değiştir.
    app: {
        resource: "TurkcellEFatura",

        einvoiceTransfer: {
            upload: "einvoice/transfer/upload"
        },

        earchiveTransfer: {
            upload: "earchive/transfer/upload"
        },

        // Outbox e-Arşiv ekranında openApi('earchive/outbox/preview'...) paterni var.
        // Bu yüzden aynı prefix’i kullanıyoruz.
        earchiveOutbox: {
            search: "earchive/outbox/search",
            setArchive: "earchive/outbox/archive-flag",
            setPaid: "earchive/outbox/paid-flag",
            cancel: "earchive/outbox/cancel",
            object: "earchive/outbox/object",
            sendMail: "earchive/outbox/send-mail",
            exportExcel: "earchive/outbox/export-excel"
        },

        einvoiceGibPortalFlag: {
            setCancelledOrObjected: "einvoice/gibportal/cancelled-or-objected-flag"
        },

        erpEarchiveCreate: {
            prefixes: "erp/earchive/prefixes",
            search: "erp/earchive/search",
            send: "erp/earchive/send",
            unitMapSave: "erp/earchive/unit-map"
        }
    },

    // ---- SAME ORIGIN / MVC içindeki “/api/..” endpointleri (relative) ----
    // (İlk paylaştığın view inline scriptleri bu formatı kullanıyordu.)
    local: {
        reports: {
            list: "/api/report-requests",
            create: "/api/report-requests/create"
        },
        sentReports: {
            list: "/api/reports/sent",
            detail: "/api/reports/detail",
            status: "/api/reports/status",
            download: "/api/reports/download"
        },
        emailTrack: {
            search: "/api/email-service/search",
            requeue: "/api/email-service/requeue"
        }
    }
};

// --- Service instance’ları ---
let gibSvc = new BaseTurkcellEfaturaApiService({
    baseType: "gibPortal",
    resource: ROUTES.gib.resource,
    withCredentials: false   // ✅ önemli
});

let appSvc = new BaseTurkcellEfaturaApiService({
    baseType: "api",
    resource: ROUTES.app.resource,
    withCredentials: false   // ✅
});

let localSvc = new BaseTurkcellEfaturaApiService({
    baseType: "relative",
    baseUrl: "",          // boş bırak
    resource: "",         // boş bırak
    allowRelative: true,  // /api/... ile çağıracağız
    withCredentials: true
});

/**
 * Genel ayar:
 *  - baseUrl’ları veya tokenProvider’ı runtime’da override etmek istersen kullan.
 *
 * @example
 * turkcellEfaturaApi.configure({
 *   gibPortalBaseUrl: "https://localhost:7151/api",
 *   apiBaseUrl: "https://localhost:7102/api/v1",
 *   tokenProvider: () => sessionStorage.getItem("token")
 * });
 */
function configure(options = {}) {
    // Token’ı 3 servise de uygula (istersen sadece 1’ine uygularsın)
    const tokenProvider = options.tokenProvider || null;
    const withCreds = options.withCredentials === true; // ✅ default false
    if (options.gibPortalBaseUrl || tokenProvider || typeof options.timeoutMs === "number") {
        gibSvc = new BaseTurkcellEfaturaApiService({
            baseType: "gibPortal",
            baseUrl: options.gibPortalBaseUrl,
            resource: ROUTES.gib.resource,
            tokenProvider,
            timeoutMs: typeof options.timeoutMs === "number" ? options.timeoutMs : 30000,
            withCredentials: withCreds
        });
    }

    if (options.apiBaseUrl || tokenProvider || typeof options.timeoutMs === "number") {
        appSvc = new BaseTurkcellEfaturaApiService({
            baseType: "api",
            baseUrl: options.apiBaseUrl,
            resource: ROUTES.app.resource,
            tokenProvider,
            timeoutMs: typeof options.timeoutMs === "number" ? options.timeoutMs : 30000,
            withCredentials: withCreds
        });
    }

    if (tokenProvider || typeof options.timeoutMs === "number") {
        localSvc = new BaseTurkcellEfaturaApiService({
            baseType: "relative",
            baseUrl: "",
            resource: "",
            allowRelative: true,
            tokenProvider,
            timeoutMs: typeof options.timeoutMs === "number" ? options.timeoutMs : 30000,
            withCredentials: withCreds
        });
    }
}

// -----------------------------
//  e-Fatura INBOX API (GİB)
// -----------------------------
export const EinvoiceInboxApi = {
    /**
     * Gelen faturaları listeler.
     * Backend: GET /api/TurkcellEFatura/einvoice/inbox/list?start&end&userId&pageIndex&pageSize&isNew
     */
    list(query) {
        return gibSvc.get(ROUTES.gib.einvoice.inboxList, query || {});
    },

    /** PDF’i yeni sekmede açmak için URL üretir. */
    pdfUrl(ettn, query) {
        return gibSvc.url(ROUTES.gib.einvoice.inboxPdf(ettn), query || {});
    },

    /** UBL/XML indirmek için URL üretir. */
    ublUrl(ettn, query) {
        return gibSvc.url(ROUTES.gib.einvoice.inboxUbl(ettn), query || {});
    },

    /** HTML önizleme için URL üretir. */
    htmlUrl(ettn, query) {
        return gibSvc.url(ROUTES.gib.einvoice.inboxHtml(ettn), query || {});
    },

    /** Gelen fatura durumunu döndürür. */
    status(ettn, query) {
        return gibSvc.get(ROUTES.gib.einvoice.inboxStatus(ettn), query || {});
    },

    /**
     * Gelen faturaya cevap gönderir (Kabul/Red vb).
     * Backend: POST /api/TurkcellEFatura/einvoice/response?userId
     *
     * Esnek kullanım:
     *  - sendApplicationResponse(ettn, query, { responseCode, note, ... })
     *  - sendApplicationResponse({ ...InvoiceResponseRequest }, query)
     */
    sendApplicationResponse(arg1, arg2, arg3) {
        // (ettn, query, body) veya (body, query)
        if (typeof arg1 === "string") {
            const ettn = arg1;
            const query = arg2 || {};
            const body = { ...(arg3 || {}) };

            // yaygın alan isimleri:
            if (body.uuid == null) body.uuid = ettn;
            if (body.ettn == null) body.ettn = ettn;

            return gibSvc.post(ROUTES.gib.einvoice.sendResponse, body, query);
        }

        // (body, query)
        const body = arg1 || {};
        const query = arg2 || {};
        return gibSvc.post(ROUTES.gib.einvoice.sendResponse, body, query);
    },

    /**
     * “Yeni” bilgisini günceller (okundu/okunmadı gibi).
     * Backend: PUT /api/TurkcellEFatura/einvoice/inbox/isnew?userId
     * body: [ { ettN/uuid/id, isNew:true|false }, ... ]
     */
    updateIsNew(items, query) {
        return gibSvc.put(ROUTES.gib.einvoice.inboxIsNewUpdate, items || [], query || {});
    },

    /**
     * Cevap gönderimi başarısız kalanları tekrar dener.
     * Backend: PUT /api/TurkcellEFatura/einvoice/response/retry?userId
     * body: [ Guid, Guid, ... ]
     */
    retryResponseList(invoiceIds, query) {
        return gibSvc.put(ROUTES.gib.einvoice.retryResponseList, invoiceIds || [], query || {});
    }
};

// -----------------------------
//  e-Fatura OUTBOX API (GİB)
// -----------------------------
export const EinvoiceOutboxApi = {
    /**
     * Giden faturaları listeler.
     * Not: Controller’ında yoksa backend eklenince çalışır.
     */
    list(query) {
        return gibSvc.get(ROUTES.gib.einvoice.outboxList, query || {});
    },

    pdfUrl(ettn, query) {
        return gibSvc.url(ROUTES.gib.einvoice.outboxPdf(ettn), query || {});
    },
    ublUrl(ettn, query) {
        return gibSvc.url(ROUTES.gib.einvoice.outboxUbl(ettn), query || {});
    },
    htmlUrl(ettn, query) {
        return gibSvc.url(ROUTES.gib.einvoice.outboxHtml(ettn), query || {});
    },

    status(ettn, query) {
        return gibSvc.get(ROUTES.gib.einvoice.outboxStatus(ettn), query || {});
    },

    reason(ettn, query) {
        return gibSvc.get(ROUTES.gib.einvoice.outboxReason(ettn), query || {});
    },

    withNullLocalReference(query) {
        return gibSvc.get(ROUTES.gib.einvoice.outboxWithNullLocalRef, query || {});
    },

    /**
     * Toplu status güncelleme
     * Backend: PUT /api/TurkcellEFatura/einvoice/outbox/statuslist?userId
     * body: { ids:[Guid], status:int }
     */
    updateStatusList(ids, status, query) {
        return gibSvc.put(
            ROUTES.gib.einvoice.outboxStatusList,
            { ids: ids || [], status: Number(status) },
            query || {}
        );
    },

    /**
     * UBL güncelleme (multipart/form-data)
     * Backend: PUT /api/TurkcellEFatura/einvoice/outbox/update-ubl?userId
     */
    updateUbl(formData, query) {
        return gibSvc.putForm(ROUTES.gib.einvoice.outboxUpdateUbl, formData, query || {});
    },

    /**
     * JSON güncelleme
     * Backend: PUT /api/TurkcellEFatura/einvoice/outbox/update-json/{ettn}?userId
     */
    updateJson(ettn, jsonBody, query) {
        return gibSvc.put(ROUTES.gib.einvoice.outboxUpdateJson(ettn), jsonBody || {}, query || {});
    },

    /**
     * JSON gönderim (invoiceId üzerinden)
     * Backend: POST /api/TurkcellEFatura/einvoice/send-json/{id}?userId&isExport&alias
     */
    sendJson(invoiceId, query) {
        return gibSvc.post(ROUTES.gib.einvoice.sendJson(invoiceId), null, query || {});
    },

    /**
     * UBL gönderim (multipart/form-data)
     * Backend: POST /api/TurkcellEFatura/einvoice/send-ubl?userId
     */
    sendUbl(formData, query) {
        return gibSvc.postForm(ROUTES.gib.einvoice.sendUbl, formData, query || {});
    },

    // ---- Aşağıdakiler bazı ekran scriptlerinde var ama backend’inde farklı olabilir.
    //      İstersen ROUTES.gib.einvoice altına ekleyip buradan yönlendirirsin.
    bulkArchive() { throw new Error("bulkArchive endpoint’i ROUTES’ta tanımlı değil."); },
    bulkPaid() { throw new Error("bulkPaid endpoint’i ROUTES’ta tanımlı değil."); },
    sendMail() { throw new Error("sendMail endpoint’i ROUTES’ta tanımlı değil."); },
    gibCancelFlag() { throw new Error("gibCancelFlag endpoint’i ROUTES’ta tanımlı değil."); }
};

// -----------------------------
//  e-Fatura Transfer (api-base)
// -----------------------------
export const EinvoiceTransferApi = {
    /**
     * UBL/ZIP aktarım yükleme (UploadTransferInvoice ekranı)
     * Backend beklenen: POST {api-base}/TurkcellEFatura/einvoice/transfer/upload?userId=...
     */
    upload(query, formData) {
        return appSvc.postForm(ROUTES.app.einvoiceTransfer.upload, formData, query || {});
    }
};

// -----------------------------
//  e-Arşiv Outbox (api-base)
// -----------------------------
export const EarchiveOutboxApi = {
    /** DataTables server-side arama. */
    search(payload) {
        return appSvc.post(ROUTES.app.earchiveOutbox.search, payload || {});
    },

    /** Seçili kayıtları “arşiv” işaretle (UI’da checkbox/buton). */
    setArchive(payload) {
        return appSvc.post(ROUTES.app.earchiveOutbox.setArchive, payload || {});
    },

    /** Seçili kayıtları “ödendi” işaretle. */
    setPaid(payload) {
        return appSvc.post(ROUTES.app.earchiveOutbox.setPaid, payload || {});
    },

    /** e-Arşiv iptal */
    cancel(payload) {
        return appSvc.post(ROUTES.app.earchiveOutbox.cancel, payload || {});
    },

    /** e-Arşiv itiraz */
    object(payload) {
        return appSvc.post(ROUTES.app.earchiveOutbox.object, payload || {});
    },

    /** Mail gönder */
    sendMail(payload) {
        return appSvc.post(ROUTES.app.earchiveOutbox.sendMail, payload || {});
    },

    /** Excel export (Blob döner) */
    exportExcel(payload) {
        return appSvc.downloadBlob(ROUTES.app.earchiveOutbox.exportExcel, null, "POST", payload || {});
    }
};

// -----------------------------
//  e-Arşiv Transfer (api-base)
// -----------------------------
export const EarchiveTransferApi = {
    /**
     * UBL/ZIP aktarım yükleme (UploadTransferEarchiveInvoice ekranı)
     * Backend beklenen: POST {api-base}/TurkcellEFatura/earchive/transfer/upload?userId=...
     */
    upload(query, formData) {
        return appSvc.postForm(ROUTES.app.earchiveTransfer.upload, formData, query || {});
    }
};

// -----------------------------
//  GİB Portal “Cancelled/Objected” flag (api-base)
// -----------------------------
export const EinvoiceGibPortalApi = {
    /**
     * “GİB Portal’da iptal/itiraz edildi” flag set eder.
     * (GibPortalCancelledOrObjected ekranı)
     */
    setCancelledOrObjectedFlag(payload) {
        return appSvc.post(ROUTES.app.einvoiceGibPortalFlag.setCancelledOrObjected, payload || {});
    }
};

// -----------------------------
//  “ROOT” API (turkcellEfaturaApi)
// -----------------------------
export const turkcellEfaturaApi = {
    configure,

    // Servisleri gerektiğinde dışarı aç (ileri seviye)
    _services: {
        gib: () => gibSvc,
        app: () => appSvc,
        local: () => localSvc
    },

    // GİB tarafı (controller’daki endpointler)
    gib: {
        static: {
            unit: (q) => gibSvc.get(ROUTES.gib.static.unit, q || {}),
            taxExemption: (q) => gibSvc.get(ROUTES.gib.static.taxExemption, q || {}),
            withholding: (q) => gibSvc.get(ROUTES.gib.static.withholding, q || {}),
            taxTypeCode: (q) => gibSvc.get(ROUTES.gib.static.taxTypeCode, q || {}),
            taxOffice: (q) => gibSvc.get(ROUTES.gib.static.taxOffice, q || {}),
            country: (q) => gibSvc.get(ROUTES.gib.static.country, q || {})
        },
        balance: {
            get: (q) => gibSvc.get(ROUTES.gib.balance.get, q || {}),
            consume: (q, body) => gibSvc.post(ROUTES.gib.balance.consume, body || {}, q || {})
        },
        einvoice: {
            inbox: EinvoiceInboxApi,
            outbox: EinvoiceOutboxApi
        }
    },

    // Local “/api/..” endpointleri (ilk view scriptlerindeki gibi)
    reports: {
        /** DataTables: POST değilse backend’ine göre düzenle */
        list: (payload) => localSvc.post(ROUTES.local.reports.list, payload || {}),
        search: (payload) => localSvc.post(ROUTES.local.reports.list, payload || {}), // alias
        create: (payload) => localSvc.post(ROUTES.local.reports.create, payload || {})
    },

    sentReports: {
        list: (payload) => localSvc.post(ROUTES.local.sentReports.list, payload || {}),
        search: (payload) => localSvc.post(ROUTES.local.sentReports.list, payload || {}), // alias
        detail: (query) => localSvc.get(ROUTES.local.sentReports.detail, query || {}),
        status: (query) => localSvc.get(ROUTES.local.sentReports.status, query || {}),
        downloadUrl: (query) => localSvc.url(ROUTES.local.sentReports.download, query || {})
    },

    earchiveEmailTrack: {
        search: (payload) => localSvc.post(ROUTES.local.emailTrack.search, payload || {}),
        requeue: (body) => localSvc.post(ROUTES.local.emailTrack.requeue, body || {})
    },

    // ERP’den e-arşiv oluştur (api-base)
    erpEarchiveCreate: {
        prefixes: (query) => appSvc.get(ROUTES.app.erpEarchiveCreate.prefixes, query || {}),
        search: (payload) => appSvc.post(ROUTES.app.erpEarchiveCreate.search, payload || {}),
        send: (payload) => appSvc.post(ROUTES.app.erpEarchiveCreate.send, payload || {}),
        saveUnitMap: (payload) => appSvc.post(ROUTES.app.erpEarchiveCreate.unitMapSave, payload || {})
    },

    // Dışarı route’ları da aç (gerekirse düzenlersin)
    routes: ROUTES
};

// Debug/legacy: window’a bas (istersen kaldır)
if (typeof window !== "undefined") {
    window.turkcellEfaturaApi = turkcellEfaturaApi;
    window.EinvoiceInboxApi = EinvoiceInboxApi;
    window.EinvoiceOutboxApi = EinvoiceOutboxApi;
    window.EinvoiceTransferApi = EinvoiceTransferApi;
    window.EarchiveOutboxApi = EarchiveOutboxApi;
    window.EarchiveTransferApi = EarchiveTransferApi;
    window.EinvoiceGibPortalApi = EinvoiceGibPortalApi;
}
