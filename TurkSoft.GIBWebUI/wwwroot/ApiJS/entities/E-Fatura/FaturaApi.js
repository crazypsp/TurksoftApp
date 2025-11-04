export const FaturaApi = {
    sendUBLInvoice: (docs) =>
        fetch('/api/EFatura/SendUBLInvoice', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(docs)
        }).then(r => r.json()),

    updateUBLInvoice: (docs) =>
        fetch('/api/EFatura/UpdateUBLInvoice', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(docs)
        }).then(r => r.json()),

    cancelUBLInvoice: (uuid, reason, cancelDate) =>
        fetch(`/api/EFatura/CancelUBLInvoice?uuid=${uuid}&reason=${reason}&cancelDate=${cancelDate}`, {
            method: 'POST'
        }).then(r => r.json()),

    queryUBLInvoice: (paramType, parameter) =>
        fetch(`/api/EFatura/QueryUBLInvoice?paramType=${paramType}&parameter=${parameter}`).then(r => r.json()),

    controlUBLXml: (xml) =>
        fetch('/api/EFatura/ControlUBLXml', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(xml)
        }).then(r => r.json()),

    getCustomerCreditCount: (vkn) =>
        fetch(`/api/EFatura/GetCustomerCreditCount?vknTckn=${vkn}`).then(r => r.json())
};
