export function getSession() {
  try {
    const raw = sessionStorage.getItem('ts.auth');
    return raw ? JSON.parse(raw) : null;
  } catch { return null; }
}
