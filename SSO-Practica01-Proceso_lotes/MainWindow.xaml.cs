﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace SSO_Practica01_Proceso_lotes
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
			//Practica 03
		public static int segundos = 0;
		public static int tiempoTranscurrido = 0;
		public static int globalMaximo = 0;
		public const int MAX_PROCESOS_LOTE = 5, TIEMPO_BLOQUEADO = 8;

		private List<int> listaId;
		private List<Proceso> fcsf, nuevos, listos, bloqueados, terminados, ejecucion;

		private Proceso procesoActual;
		private int tiempoMaxEstimado;
        private bool estaCorriendo,
                     estaPausado,
					 yaTermino;

		DispatcherTimer dt;
		Random rnd;

		public MainWindow()
		{
			InitializeComponent();

			cmbOperacion.Items.Add("+");
			cmbOperacion.Items.Add("-");
			cmbOperacion.Items.Add("*");
			cmbOperacion.Items.Add("/");
			cmbOperacion.Items.Add("%");
			cmbOperacion.Items.Add("^");
			cmbOperacion.Items.Add("%%");
			cmbOperacion.SelectedIndex = 0;

			for (int i = 0; i < 15; i++)
			{
				cmbOperando1.Items.Add(i);
				cmbOperando2.Items.Add(i);
			}

			for (int i = 1; i < 30; i++)
			{
				cmbNumeroProcesos.Items.Add(i);
			}

			fcsf = new List<Proceso>();
			nuevos = new List<Proceso>();
			listos = new List<Proceso>();
			bloqueados = new List<Proceso>();
			terminados = new List<Proceso>();
			ejecucion = new List<Proceso>();

			cmbOperando1.SelectedIndex = 0;
			cmbOperando2.SelectedIndex = 0;
			cmbNumeroProcesos .SelectedIndex = 0;

			dt = new DispatcherTimer();

			dt.Tick += dispatcherTimer_Tick;
			dt.Interval = new System.TimeSpan(0, 0, 1);

			listaId = new List<int>();
			rnd = new Random();

            yaTermino = estaPausado = estaCorriendo = false;
            
			tiempoMaxEstimado = 0;

			actualizaGridView();
		}

		private void dispatcherTimer_Tick(object sender, EventArgs e)
		{
			segundos++;

			txbCronometro.Text = segsToTime(segundos);

			//En ejecucion
			procesoActual.TSer++;

			var restante = procesoActual.TME;

			txbRestante.Text = segsToTime(restante);

			txbTranscurrido.Text = segsToTime(tiempoTranscurrido);

			procesoActual.TME --;
			tiempoTranscurrido++;


			if (procesoActual.TME <= 0)
			{
				siguienteProceso();
			}

			procesaBloqueados();

			actualizaGridView();

			//dgvProcesos.Items.Refresh();
		}

		private void procesaBloqueados()
		{
			List<Proceso> liberados = new List<Proceso>();

			foreach (var proceso in bloqueados)
			{
				proceso.Bloq--;

				if(proceso.Bloq <= 0 )
				{
					liberados.Add(proceso);
				}
			}

			foreach (var liberado in liberados)
			{
				bloqueados.Remove(liberado);
				listos.Add(liberado);
			}
		}

		private void siguienteProceso( bool bloqueado = false)
		{
			if (!bloqueado)
			{
				procesoActual.termino(segundos);
				terminados.Add(procesoActual);
			}
			ejecucion.Remove(procesoActual);

			if (listos.Count > 0)
			{
				procesoActual = listos[0];

				listos.Remove(procesoActual);

				if (nuevos.Count > 0)
				{
					listos.Add(nuevos[0]);
					nuevos.Remove(nuevos[0]);
				}

				ejecucion.Add(procesoActual);
				procesoActual.ejecuto(segundos);
			}
			else if (bloqueados.Count <= 0)
			{
				dt.Stop();				
				cambiarEstadoDataGrid();
				txbEstado.Text = "Terminado";
				yaTermino = true;
			}			

			actualizaGridView();

		}

		private void btnIniciarCronometro_Click(object sender, RoutedEventArgs e)
		{
			btnIniciarCronometro.IsEnabled = false;

			procesoActual = listos[0];

			listos.Remove(procesoActual);
			ejecucion.Add(procesoActual);
			procesoActual.ejecuto(segundos);

			cambiarEstadoDataGrid();

			dt.Start();

			this.Focus();
            estaCorriendo = true;
			txbEstado.Text = "Corriendo";
		}

		private void actualizaGridView()
		{
			dgvNuevos.ItemsSource = null;
			dgvListos.ItemsSource = null;
			dgvBloqueados.ItemsSource = null;
			dgvTerminados.ItemsSource = null;
			dgvEjecucion.ItemsSource = null;

			dgvNuevos.ItemsSource = nuevos;
			dgvListos.ItemsSource = listos;
			dgvTerminados.ItemsSource = terminados;
			dgvBloqueados.ItemsSource = bloqueados;
			dgvEjecucion.ItemsSource = ejecucion;
		}

		private void btnCrearProceso_Click(object sender, RoutedEventArgs e)
		{
			int numeroProcesos = int.Parse(cmbNumeroProcesos.SelectedItem.ToString());

			for (int i = 0; i < numeroProcesos; i++)
			{
				crearProceso
					(
						"A",
						cmbOperacion.Items[rnd.Next(0,cmbOperacion.Items.Count - 1 )].ToString(),
						rnd.Next(100).ToString(),
						rnd.Next(1,100).ToString(),
						rnd.Next(1,15)
					);
				if (i < 5)
					listos.Add(fcsf[i]);
				else
					nuevos.Add(fcsf[i]);
			}

			btnCrearProceso.IsEnabled = false;
			actualizaGridView();
		}

		private void crearProceso(string programador, string Op, string operando1, string operando2, int tiempo)
		{
			Proceso nuevoProceso = new Proceso
					(
						programador,
						Op,
						operando1,
						operando2,
						tiempo
					);

			//listaId.Add(id);

			globalMaximo += nuevoProceso.TME;

			fcsf.Add(nuevoProceso);

			txbMaximoGlobal.Text = segsToTime(globalMaximo);
		}

		private string segsToTime(int segundos)
		{
			return 	(globalMaximo / 60).ToString().PadLeft(2, '0') + ":" + (globalMaximo % 60).ToString().PadLeft(2, '0');
		}

		private void dgvLotes_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			
		}

		private void cambiarEstadoDataGrid()
		{
			dgvNuevos.IsEnabled = !dgvNuevos.IsEnabled;
			dgvListos.IsEnabled = !dgvNuevos.IsEnabled;
			dgvBloqueados.IsEnabled = !dgvBloqueados.IsEnabled;
			dgvTerminados.IsEnabled = !dgvTerminados.IsEnabled;
			dgvEjecucion.IsEnabled = !dgvEjecucion.IsEnabled;
		}

        private void mandarBloqueado()
        {
			procesoActual.Bloq = TIEMPO_BLOQUEADO;

			bloqueados.Add(procesoActual);
			ejecucion.Remove(procesoActual);

			siguienteProceso(true);			
        }    

		private void cambiarEstadoGUI()
		{
			
		}

		private void Grid_KeyDown(object sender, KeyEventArgs e)
		{
			//MessageBox.Show(e.Key.ToString());
		}

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            var procesoAnterior = procesoActual;

			if (yaTermino)
				return;

            else if (estaCorriendo && e.Key == Key.W)
            {
				globalMaximo -= procesoActual.TME;
                siguienteProceso();
                procesoAnterior.Res = "Error";
				txbMaximoGlobal.Text = segsToTime(globalMaximo);
            }
            else if (estaCorriendo && e.Key == Key.P)
            {
                dt.Stop();
                estaPausado = true;
				txbEstado.Text = "Pausado";
                //cambiarEstadoDataGrid();
            }
            else if ( estaPausado && e.Key == Key.C )
            {
                dt.Start();
                estaPausado = false;
				txbEstado.Text = "Corriendo";
                //cambiarEstadoDataGrid();
            }

            else if ( estaCorriendo && e.Key == Key.E)
            {
                mandarBloqueado();
            }
        }
    }

    class Proceso
	{
		public static int idProceso = 1;

		public int Id { get; set; }
		//public String Programador { get; set; }
		public String Op1 { get; set; }
		public String Op { get; set; }
		public String Op2 { get; set; }
		public string Res { get; set; }		
		public bool Termino { get; set; }
		public int TME { get; set; }
		public int TL { get; set; }
		public int TFin { get; set; }
		public int TRet { get; set; }
		public int TResp { get; set; }
		public int TEsp { get; set; }
		public int TSer { get; set; }
		public int Bloq { get; set; }
		private int servidoPrimeraVez = -1;

		public Proceso(String Programador, String Op, String Op1, String Op2, int ETA)
		{
			this.Id = idProceso;
			//this.Id = id;
			//this.Programador = Programador;
			this.Op = Op;
			this.Op1 = Op1;
			this.Op2 = Op2;
			this.TME = ETA;
			this.Termino = false;
			idProceso++;
		}

		public void termino(int tiempoActual)
		{
			this.Termino = true;
			this.TFin = tiempoActual;
			this.TRet = this.TFin - this.TL;
			this.TResp = this.TL;
		}

		public void ejecuto(int tiempoActual)
		{
			if (servidoPrimeraVez == -1)
				servidoPrimeraVez = tiempoActual;

			this.TResp = servidoPrimeraVez - TL;
		}

		public void resolverEcuacion()
		{
			int op1 = int.Parse(Op1),
				op2 = int.Parse(Op2);

			switch (Op)
			{
				case "+":
					Res = (op1 + op2).ToString();
					break;
				case "-":
					Res = (op1 - op2).ToString();
					break;
				case "*":
					Res = (op1 * op2).ToString();
					break;
				case "/":
					Res = (op1 / op2).ToString();
					break;
				case "%":
					Res = (op1 % op2).ToString();
					break;
				case "^":
					Res = (op1 ^ op2).ToString();
					break;
				case "%%":
					Res = ((op1 / 100) * op2).ToString();
					break;
				default:
					Res = (-1).ToString();
					break;
			}
		}
	}
}