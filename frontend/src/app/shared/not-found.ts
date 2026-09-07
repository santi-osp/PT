import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  imports: [RouterLink],
  template: `<section class="state-panel state-panel--roomy"><span class="state-panel__code">404</span><h1>Página no encontrada</h1><p>La ruta que buscas no existe en DataGo.</p><a routerLink="/clientes" class="button button--primary">Volver a clientes</a></section>`
})
export class NotFound {}
