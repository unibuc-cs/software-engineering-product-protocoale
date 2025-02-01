Non-functional requirements

- Responsiveness

    Produsul nostru este responsive, reusind sa intoarca rezultate de la mai multe magazine intr-un timp cu mult
mai scurt decat daca userul ar cauta produsele manual. In schimb acest factor depinde si de calitatea internetului si
de responsiveness-ul siteurilor (Mega, Auchan si Carrefour).

- Reliability

    Produsul functioneaza corect, iar din testele facute am rezolvat bug-urile gasite. La ultima testare nu existau bug-uri si toate feature-urile sunt functionale.

- Availability

    Produsul nostru poate sustine mai multi useri in paralel. In schimb, daca uzerii sunt activi si fac multe cautari este posibil ca userii sa experimenteze lag.

- Security

    Produsul nostru nu foloseste datele userilor. Toate actiunile, cu exceptia cautarilor, se petrec local. Iar sistemul este suficient de robust la atacuri de securitate. (vezi security-audit.md)

- Usability

    Userii au access la toate functionalitatile, foarte eficient. Pentru a putea avea access la harta, in schimb, trebuie sa permita accesul la locatie. Locatie pe care o va folosi API-ul de la Google Maps.

- Maintainability

    In partea de mententanta lucrurile nu sunt complicate. Este posibil ca din cand in cand sa fie nevoie de curatarea bazei de date pentru a nu trece de o limita in care sa devina un impediment pentru responsiveness. Iar pentru update-uri, acestea se pot face foarte rapid, dar necesita restartarea serverului.

- Resilience

    Produsul poate sa continue functionarea in cazul unor erori (citiri/scrieri din baza de date, Google API, sau pica unul din siteurile magazinelor). Dar pe partea de atac cibernetic, acesta prezinta un posibil risc de DOS (vezi security-audit.md).

