export const DraftsApi = {

    // ----------------------------------------------------------
    // 1️⃣ TASLAK FATURA LİSTELEME
    // /api/EFatura/QueryUBLInvoice?paramType=documentStatus&parameter=Draft
    // ----------------------------------------------------------
    listDrafts: () =>
        fetch('/api/EFatura/QueryUBLInvoice?paramType=documentStatus&parameter=Draft')
            .then(r => r.json()),

    // ----------------------------------------------------------
    // 2️⃣ TASLAK FATURA GÖNDERME
    // /api/EFatura/SendUBLInvoice
    // ----------------------------------------------------------
    sendUBLInvoice: (docs) =>
        fetch('/api/EFatura/SendUBLInvoice', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(docs)
        }).then(r => r.json()),

    // ----------------------------------------------------------
    // 3️⃣ TASLAK FATURA XML KONTROLÜ
    // /api/EFatura/ControlUBLXml
    // ----------------------------------------------------------
    controlUBLXml: (xml) =>
        fetch('/api/EFatura/ControlUBLXml', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(xml)
        }).then(r => r.json()),

    // ----------------------------------------------------------
    // 4️⃣ TASLAK FATURA SİLME (uygulama içi)
    // /api/Invoices/Delete?id={id}
    // ----------------------------------------------------------
    deleteDraft: (id) =>
        fetch(`/api/Invoices/Delete?id=${id}`, {
            method: 'DELETE'
        }).then(r => r.json()),

    // ----------------------------------------------------------
    // 5️⃣ KREDİ (KONTÖR) SORGULAMA
    // /api/EFatura/GetCustomerCreditCount?vknTckn=
    // ----------------------------------------------------------
    getCustomerCreditCount: (vkn) =>
        fetch(`/api/EFatura/GetCustomerCreditCount?vknTckn=${vkn}`)
            .then(r => r.json())
};
