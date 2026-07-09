# User Stories

## Public Side

### Story 1: Browse menu

As a guest, I want to browse menu items by category and dietary needs so I can quickly find food I can order.

**Acceptance Criteria**

- Menu items are visible on a public page
- I can filter by category
- I can filter by maximum price
- I can search allergens text

### Story 2: Request a reservation

As a guest, I want to submit a reservation request so the restaurant can confirm my table.

**Acceptance Criteria**

- Reservation form captures name, email, phone, date, party size, and notes
- New reservations default to `Pending`
- User sees confirmation after submit

### Story 3: Send a private event inquiry

As a guest, I want to request a private event so the restaurant can contact me with details.

**Acceptance Criteria**

- Form captures event date, party size, and message
- Inquiry is stored in the database

### Story 4: Contact the restaurant

As a guest, I want to send a contact message so I can ask general questions.

**Acceptance Criteria**

- Contact form validates required fields
- Message is stored in the database

## Admin Side

### Story 5: Manage menu items

As an admin, I want to add, edit, and remove menu items so the menu stays current.

**Acceptance Criteria**

- Admin can create a menu item
- Admin can edit all menu fields
- Admin can delete a menu item

### Story 6: Review reservations

As an admin, I want to review and update reservation statuses so front-of-house operations stay organized.

**Acceptance Criteria**

- Admin can view all reservations
- Admin can mark a reservation as pending, confirmed, or canceled

### Story 7: Export weekly report

As an admin, I want a weekly reservation export so I can review booking trends outside the app.

**Acceptance Criteria**

- Admin can download a CSV for the selected week
- Export includes date, party size, status, and estimated revenue

### Story 8: See dashboard insights

As an admin, I want to see estimated revenue and traffic patterns so I can plan staffing and promotions.

**Acceptance Criteria**

- Dashboard shows estimated weekly revenue
- Dashboard shows popular menu categories
- Dashboard shows busiest reservation days
