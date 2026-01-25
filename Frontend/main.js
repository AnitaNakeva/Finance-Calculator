document.addEventListener("DOMContentLoaded", () => {
    const buttons = document.querySelectorAll(".nav-btn");
    const panels = document.querySelectorAll(".panel");

    buttons.forEach(btn => {
      btn.addEventListener("click", () => {
        buttons.forEach(b => b.classList.remove("active"));
        panels.forEach(p => p.classList.remove("active"));

        btn.classList.add("active");
        const id = btn.dataset.tab;
        const panel = document.getElementById(id);
        if (panel) panel.classList.add("active");
      });
    });
  });

  function clearResult(id){
    const el = document.getElementById(id);
    if (el) el.textContent = "—";
  }

  function showError(preId, msg){
    const el = document.getElementById(preId);
    if (el) el.textContent = `Грешка: ${msg}`;
  }

  function isValidNumber(n){
    return typeof n === "number" && !Number.isNaN(n) && Number.isFinite(n);
  }

  async function postJson(url, data){
    const res = await fetch(url, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(data)
    });

    const text = await res.text();
    let json = null;
    try { json = text ? JSON.parse(text) : null; } catch { /* ignore */ }

    if (!res.ok) {
      // ако бекендът връща { message: "..."} или validation errors
      const message =
        (json && (json.message || json.title)) ||
        `HTTP ${res.status}`;
      throw new Error(message);
    }

    return json;
  }
