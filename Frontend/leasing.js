// if (![price, downPayment, months].every(isValidNumber) || price <= 0 || months <= 0 || downPayment < 0) {
//   showError("leaseResult", "Попълни коректни стойности.");
//   return;
// }
//
// if (downPayment > price) {
//   showError("leaseResult", "Първоначалната вноска не може да е по-голяма от цената.");
//   return;
// }
//
// const data = { price, downPayment, months };
async function calculateLeasing() {
  const itemPrice = Number(document.getElementById("ItemPrice").value);
  const downPayment = Number(document.getElementById("DownPayment").value);
  const termMonths = Number(document.getElementById("TermMonths").value);
  const monthlyPayment = Number(document.getElementById("MonthlyPayment").value);
  const processingFeePercent = Number(document.getElementById("ProcessingFeePercent").value);


const data ={
  itemPrice:itemPrice,
    downPayment:downPayment,
    termMonths:termMonths,
    monthlyPayment:monthlyPayment,
    processingFeePercent:processingFeePercent,
  };


  try {
    const result = await postLeasing(data);
    document.getElementById("leaseResult").textContent = JSON.stringify(result, null, 2);
  } catch (err) {
    showError("leaseResult", err.message);
  }
}
