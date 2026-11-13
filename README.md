# Scan & Eat — Interactive demo

This branch is a static portfolio demo with 38 sample dishes and bundled WebP photos. It runs on Vercel or a local static server with no build step, API, or database.

## Try the full demo

1. Open **Explore the menu**, search for a dish, and add it to the cart.
2. Add a preparation note and send the order.
3. Open **Kitchen** and **Cashier** from the landing page in the same browser. Follow the ticket in Kitchen, then select the order in Cashier and choose **Print check**.
4. Open **Manager → Tables** to generate a table QR, open its menu, or disable it. Try changing a price in **Products** and check the guest menu.

Demo data is stored in the browser and shared across tabs on the same origin. A separate browser or device has its own data. Manager and staff screens are open directly from the launcher in this demo.

## Run locally

From the branch root, run `python -m http.server 8000` and open `http://localhost:8000/`.

## Deploy this branch

Import the GitHub repository into Vercel and configure `front-Deploy` as the production branch. Use the directory containing `index.html` and `vercel.json` as the root. Use the **Other** framework preset and serve the static files without an install or build command. The Vercel configuration defines short routes for the four workspaces and response headers.

The demo is enabled by the empty `BACKEND_URL` values in `customer/js/config.js` and `shared/js/config.js`. See [`front-end`](../../tree/front-end) for the API-connected frontend.
