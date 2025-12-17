// wwwroot/apps/reports/ReportSendingRequest.js
import { turkcellEfaturaApi } from '../Base/turkcellEfaturaApi.js';

(function () {
    "use strict";

    if (!window.jQuery) {
        console.error("[ReportSendingRequest] jQuery bulunamadı.");
        return;
    }
    const $ = window.jQuery;

    const $table = $("#reportDatatable");
    const $btnNew = $("#generateNewArchiveReport");

    let dt = null;

    function notifyOk(m) {
        if (window.toastr?.success) toastr.success(m);
        else alert(m);
    }
    function notifyErr(m) {
        if (window.toastr?.error) toastr.error(m);
        else alert(m);
    }

    function html(s) {
        return String(s ?? "")
            .replace(/&/g, "&amp;").replace(/</g, "&lt;")
            .replace(/>/g, "&gt;").replace(/"/g, "&quot;")
            .replace(/'/g, "&#39;");
    }

    function fmtDate(v) {
        if (!v) return "";
        try {
            const d = (typeof v === "string" && v.includes(" ") && !v.includes("T"))
                ? new Date(v.replace(" ", "T"))
                : new Date(v);
            if (Number.isNaN(d.getTime())) return html(v);
            return d.toLocaleString("tr-TR");
        } catch {
            return html(v);
        }
    }

    function normalizeRows(resp) {
        // destek: Array | {items:[]} | {data:[]} | {result:{items:[]}} | {result:{data:[]}}
        if (!resp) return [];
        if (Array.isArray(resp)) return resp;
        if (Array.isArray(resp.items)) return resp.items;
        if (Array.isArray(resp.data)) return resp.data;
        if (resp.result && Array.isArray(resp.result.items)) return resp.result.items;
        if (resp.result && Array.isArray(resp.result.data)) return resp.result.data;
        return [];
    }

    function mapRow(r) {
        const requestTime = r.requestTime || r.RequestTime || r.createdAt || r.CreatedAt || r.requestDate || r.RequestDate;
        const period = r.period || r.Period || r.term || r.Term || r.monthPeriod || r.MonthPeriod;
        const status = r.status || r.Status || r.state || r.State;

        return {
            RequestTime: requestTime,
            Period: period,
            Status: status
        };
    }

    function renderStatus(s) {
        const up = String(s ?? "").toUpperCase();
        let cls = "label-default";
        if (up === "GÖNDERİLDİ" || up === "SENT" || up === "BASARILI" || up === "SUCCESS") cls = "label-success";
        else if (up === "GÖNDERİLECEK" || up === "OLUŞTURULDU" || up === "CREATED" || up === "PENDING") cls = "label-primary";
        else if (up === "HATA" || up === "ERROR" || up === "FAILED") cls = "label-danger";
        return `<span class="label ${cls}">${html(up || s || "")}</span>`;
    }

    // ====== API resolve + fallback fetch ======

    function getApiBase() {
        const meta = document.querySelector('meta[name="api-base"]');
        if (meta?.content) return meta.content.replace(/\/+$/, '');
        if (window.__API_BASE) return String(window.__API_BASE).replace(/\/+$/, '');
        const body = document.getElementById('MainBody');
        if (body?.dataset?.apiBase) return String(body.dataset.apiBase).replace(/\/+$/, '');
        return '';
    }

    async function fetchJson(path, { method = "GET", body = null } = {}) {
        const base = getApiBase();
        if (!base) throw new Error("API base bulunamadı (meta api-base).");

        const url = `${base}/TurkcellEFatura/${path.replace(/^\/+/, "")}`;
        const opts = {
            method,
            credentials: "same-origin",
            headers: { "Accept": "application/json" }
        };
        if (body != null) {
            opts.headers["Content-Type"] = "application/json";
            opts.body = JSON.stringify(body);
        }

        const resp = await fetch(url, opts);
        const text = await resp.text();

        let data = null;
        try { data = text ? JSON.parse(text) : null; } catch { data = text; }

        if (!resp.ok) {
            const msg = (data && data.message) ? data.message : (typeof data === "string" ? data : `HTTP ${resp.status}`);
            const err = new Error(msg);
            err.status = resp.status;
            err.data = data;
            throw err;
        }

        return data;
    }

    function pickFn(obj, names) {
        for (const n of names) {
            const fn = obj?.[n];
            if (typeof fn === "function") return fn;
        }
        return null;
    }

    function getReportsApi() {
        // olası namespace varyasyonları
        return (
            turkcellEfaturaApi?.reports ||
            turkcellEfaturaApi?.Reports ||
            turkcellEfaturaApi?.report ||
            turkcellEfaturaApi?.Report ||
            null
        );
    }

    async function apiListSendingRequests() {
        const reports = getReportsApi();

        // 1) Base kütüphane fonksiyon adlarını dene
        if (reports) {
            const fn = pickFn(reports, [
                "listSendingRequests",
                "listSendingRequest",
                "listReportSendingRequests",
                "listRequests",
                "list",
                "getSendingRequests",
                "getList"
            ]);
            if (fn) return await fn.call(reports);
        }

        // 2) Base yoksa / fonksiyon yoksa: backend fetch fallback (sırayla dene)
        const candidates = [
            { method: "GET", path: "reports/sendingrequests" },
            { method: "GET", path: "reports/sendingrequest/list" },
            { method: "GET", path: "reports/sendingrequests/list" },
            { method: "GET", path: "reports/sendingrequest" },
            { method: "GET", path: "reports/list" }
        ];

        let lastErr = null;
        for (const c of candidates) {
            try {
                return await fetchJson(c.path, { method: c.method });
            } catch (e) {
                lastErr = e;
                // 404/405 -> sıradakini dene
                if (e?.status === 404 || e?.status === 405) continue;
                // diğer hatalarda direkt fırlat (yetki, 500 vs.)
                throw e;
            }
        }

        throw lastErr || new Error("Rapor talep listesi endpoint'i bulunamadı.");
    }

    async function apiCreateSendingRequest(payload) {
        const reports = getReportsApi();

        // 1) Base kütüphane fonksiyon adlarını dene
        if (reports) {
            const fn = pickFn(reports, [
                "createSendingRequest",
                "createSendRequest",
                "createRequest",
                "generateNewArchiveReport",
                "create",
                "post"
            ]);
            if (fn) return await fn.call(reports, payload ?? {});
        }

        // 2) backend fetch fallback
        const candidates = [
            { method: "POST", path: "reports/sendingrequests" },
            { method: "POST", path: "reports/sendingrequest/create" },
            { method: "POST", path: "reports/sendingrequest" },
            { method: "POST", path: "reports/create" }
        ];

        let lastErr = null;
        for (const c of candidates) {
            try {
                return await fetchJson(c.path, { method: c.method, body: payload ?? {} });
            } catch (e) {
                lastErr = e;
                if (e?.status === 404 || e?.status === 405) continue;
                throw e;
            }
        }

        throw lastErr || new Error("Rapor talebi oluşturma endpoint'i bulunamadı.");
    }

    // ====== DataTable ======

    function ensureDataTable() {
        if (!$.fn.DataTable) return;
        if (dt) return;

        dt = $table.DataTable({
            data: [],
            serverSide: false,
            processing: true,
            searching: false,
            order: [[0, "desc"]],
            pageLength: 25,
            lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]],
            language: { url: "//cdn.datatables.net/plug-ins/1.13.6/i18n/tr.json" },
            columns: [
                { data: "RequestTime", render: (v) => fmtDate(v) },
                { data: "Period", defaultContent: "" },
                { data: "Status", render: (v) => renderStatus(v) },
            ],
            deferRender: true
        });
    }

    function renderPlain(rows) {
        const $tb = $table.find("tbody");
        $tb.empty();
        rows.forEach(r => {
            $tb.append(
                `<tr>
                    <td>${fmtDate(r.RequestTime)}</td>
                    <td>${html(r.Period || "")}</td>
                    <td>${renderStatus(r.Status)}</td>
                </tr>`
            );
        });
    }

    async function loadList() {
        try {
            const resp = await apiListSendingRequests();
            const rows = normalizeRows(resp).map(mapRow);

            ensureDataTable();
            if (dt) {
                dt.clear().rows.add(rows).draw(false);
            } else {
                renderPlain(rows);
            }
        } catch (e) {
            console.error("[ReportSendingRequest] list error:", e);
            notifyErr(e?.message || "Rapor talep listesi alınamadı.");

            ensureDataTable();
            if (dt) dt.clear().draw();
            else renderPlain([]);
        }
    }

    function setConfirmBusy(busy) {
        const $btn = $("#ReportConfirm");
        if (!$btn.length) return;

        if (busy) {
            $btn.data("busy", true).prop("disabled", true)
                .html('<i class="fa fa-spinner fa-spin"></i> Oluşturuluyor...');
        } else {
            $btn.data("busy", false).prop("disabled", false).text("Oluştur");
        }
    }

    async function createRequest() {
        try {
            setConfirmBusy(true);

            // period vb. eklersen payload’a koyarız
            await apiCreateSendingRequest({});

            if ($("#newReportModal").length) $("#newReportModal").modal("hide");

            notifyOk("Yeni rapor talebi oluşturuldu.");
            await loadList();
        } catch (e) {
            console.error("[ReportSendingRequest] create error:", e);
            notifyErr(e?.message || "Rapor talebi oluşturulamadı.");
        } finally {
            setConfirmBusy(false);
        }
    }

    function bindEvents() {
        $btnNew.on("click", function () {
            if ($("#newReportModal").length) {
                $("#newReportModal").modal("show");
            } else {
                if (confirm("Yeni rapor talebi oluşturulsun mu?")) {
                    createRequest();
                }
            }
        });

        $("#ReportConfirm").on("click", function () {
            if ($(this).data("busy")) return;
            createRequest();
        });

        // Eski onclick’ler bozulmasın
        window.reportSend = function () {
            createRequest();
        };

        $("#newReportModal").on("hidden.bs.modal", function () {
            setConfirmBusy(false);
        });
    }

    function init() {
        bindEvents();
        loadList();
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", init);
    } else {
        init();
    }
})();
