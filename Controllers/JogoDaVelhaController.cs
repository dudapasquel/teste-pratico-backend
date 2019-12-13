using APIJogoDaVelha.Model;
using Dados;
using Microsoft.AspNetCore.Mvc;
using Servico;
using Servico.Util;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace APIJogoDaVelha.Controllers
{
    [ApiController]
    [Microsoft.AspNetCore.Mvc.Route("[controller]")]
    public class JogoDaVelhaController : ControllerBase
    {
        private List<Partida> partidas;
        private JogoDaVelhaServico jogoDaVelhaServico;
        private AppDomain appDomain;
        private const string CODIGO_ARMAZENAR_PARTIDAS = "partidas";

        public JogoDaVelhaController()
        {
            appDomain = AppDomain.CurrentDomain;
            jogoDaVelhaServico = new JogoDaVelhaServico();
            var partidasArmazenadas = appDomain.GetData(CODIGO_ARMAZENAR_PARTIDAS);
            partidas = partidasArmazenadas != null ? (List<Partida>)partidasArmazenadas : new List<Partida>();
        }

        [Microsoft.AspNetCore.Mvc.HttpPost]
        [Microsoft.AspNetCore.Mvc.Route("game")]
        public ActionResult InicializarPartida()
        {
            var partida = jogoDaVelhaServico.InicializarPartida();
            partidas.Add(partida);
            appDomain.SetData(CODIGO_ARMAZENAR_PARTIDAS, partidas);
            return new JsonResult(new { id = partida.Id, firstPlayer = partida.FirstPlayer });
        }

        [Microsoft.AspNetCore.Mvc.HttpPost]
        [Microsoft.AspNetCore.Mvc.Route("game/{id}/movement")]
        public ActionResult EfetuarJogada([FromRoute] string id, [Microsoft.AspNetCore.Mvc.FromBody]Jogada model)
        {
            model.Player = model.Player.ToUpper();

            if(model.Player != Jogador.PLAYER_1 && model.Player != Jogador.PLAYER_2)
                return BadRequest(new { msg = "Jogador inválido" });

            var retornoJogada = jogoDaVelhaServico.EfetuarJogada(partidas, id, model.Player, model.Position.x, model.Position.y);

            if (retornoJogada == null) { 
                return BadRequest();
            }
            else if (retornoJogada.Status == 200 && string.IsNullOrWhiteSpace(retornoJogada.Mensagem))
            {
                partidas = retornoJogada.PartidasAtualizadas;
                appDomain.SetData(CODIGO_ARMAZENAR_PARTIDAS, partidas);
                return Ok();
            }
            else if (retornoJogada.Status == 200 && string.IsNullOrWhiteSpace(retornoJogada.Vitorioso))
            {
                partidas = retornoJogada.PartidasAtualizadas;
                appDomain.SetData(CODIGO_ARMAZENAR_PARTIDAS, partidas);
                return Ok(new { msg = retornoJogada.Mensagem });
            }
            else if (retornoJogada.Status == 200 && !string.IsNullOrWhiteSpace(retornoJogada.Vitorioso))
            {
                partidas = retornoJogada.PartidasAtualizadas;
                appDomain.SetData(CODIGO_ARMAZENAR_PARTIDAS, partidas);
                return Ok(new { msg = retornoJogada.Mensagem, winner = retornoJogada.Vitorioso });
            }
            else if (retornoJogada.Status == 400)
            {
                return BadRequest(new { msg = retornoJogada.Mensagem });
            }
            else if (retornoJogada.Status == 404) { 
                return NotFound(new { msg = retornoJogada.Mensagem });
            }
            else { 
                return BadRequest();
            }
        }
    }

}
