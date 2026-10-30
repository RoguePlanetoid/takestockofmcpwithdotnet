# Error path examples

## Product not found

No product matched "brake disc". Try a broader search or provide the product code.

## Multiple products found

I found more than one brake pad product. Which product code should I use?

- BP123 Premium Brake Pad
- BP124 Economy Brake Pad

## Insufficient stock

Cannot sell 25 of BP124 Economy Brake Pad because only 15 are in stock.

The tool returns MCP `isError: true` with text content containing:

```json
{"error":"Cannot sell 25 of BP124 Economy Brake Pad because only 15 are in stock."}
```
