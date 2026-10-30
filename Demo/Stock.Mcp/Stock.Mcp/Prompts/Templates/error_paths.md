Handle stock-control errors clearly.

Useful demo error paths:

- Product not found: suggest search_products with a broader query.
- Multiple matching products: ask the user to choose a product code.
- Missing create-product values: ask for productCode, costPrice, or sellPrice.
- Insufficient stock: state available stock and requested quantity.
- Invalid quantity: stock update quantities must be zero or greater; sale quantities must be greater than zero.
- Invalid stock mode: use only Add, Remove, or Set.

Business failures are returned with MCP isError: true and a JSON error message. Explain that message; do not pretend the operation succeeded.
