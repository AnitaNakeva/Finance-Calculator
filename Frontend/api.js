const API_BASE_URL = window.API_BASE_URL || "http://localhost:5195";

// ======= CREDIT =======
async function postCredit(data) {
  const response = await fetch(`${API_BASE_URL}/api/credit/calculate`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });

  if (!response.ok) throw new Error("Грешка при кредитния калкулатор");
  return await response.json();
}

// ======= REFINANCE =======
async function postRefinance(data) {
  const response = await fetch(`${API_BASE_URL}/api/refinance/calculate`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });

  if (!response.ok) throw new Error("Грешка при рефинансиране");
  return await response.json();
}

// ======= LEASING =======
async function postLeasing(data) {
  const response = await fetch(`${API_BASE_URL}/api/leasing-goods/calculate`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });

  if (!response.ok) throw new Error("Грешка при лизинг");
  return await response.json();
}
