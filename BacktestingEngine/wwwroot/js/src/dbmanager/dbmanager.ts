

function renderDatabaseTable(dto: DbCandleSummaryResponseDto){
    const template = document.getElementById("datasetRowTemplate") as HTMLTemplateElement;
    const tablerow = template.content.cloneNode(true);

    
    tr.cells[0].textContent = dto.symbol;
    tr.cells[1].textContent = dto.interval;
    tr.cells[2].textContent = dto.firstOpenTimeMs.toLocaleString();
    tr.cells[3].textContent = dto.lastOpenTimeMs.toLocaleString();
    tr.cells[4].textContent = dto.candlesCount.toString();
}