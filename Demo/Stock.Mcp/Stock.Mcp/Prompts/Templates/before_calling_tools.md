Before calling stock MCP tools:

- Resolve the product code where possible.
- The demo catalogue uses English descriptions. Keep product codes unchanged; translate description searches into English where needed and use a short product term.
- If no products match, try a singular form, synonym, or broader term. The server does not translate or change word forms.
- Use only products returned by the tools; do not invent product codes or assume a translation matched.
- If a description matches multiple products, ask the user to choose one.
- Do not create a product until productCode, description, costPrice, and sellPrice are known.
- Use search_products with maxStock for low-stock requests.
- Use update_stock with mode Add, Remove, or Set for stock adjustments.
- Use record_sale for sold items because it records the sale and reduces stock.
- If a request would reduce stock below zero, explain the issue and do not continue.
